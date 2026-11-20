using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.eShopWeb.IntegrationTests.Fixtures;

/// <summary>
/// Reads a database's schema as sorted, one-line descriptions, so comparing two schemas shows
/// exactly which lines differ.
/// </summary>
public static class SqlServerSchema
{
    // Each query returns one nvarchar column named Value. The catalog views mix collations, so
    // system-supplied strings are pinned to the database collation before they are concatenated.
    // Constraint and index names are kept, since a renamed key or index is drift too.
    private const string TablesQuery = """
        SELECT CONCAT(SCHEMA_NAME(t.schema_id) COLLATE DATABASE_DEFAULT, '.', t.name) AS Value
        FROM sys.tables t
        WHERE t.name <> '__EFMigrationsHistory'
        """;

    // DEFAULT stays last on each column line: the drift test matches known defaults by suffix.
    private const string ColumnsQuery = """
        SELECT CONCAT(t.name, '.', c.name, ': ', ty.name COLLATE DATABASE_DEFAULT,
            '(', c.max_length, ',', c.precision, ',', c.scale, ')',
            CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END,
            CASE WHEN c.is_identity = 1 THEN CONCAT(' IDENTITY(',
                CAST(idc.seed_value AS nvarchar(40)), ',', CAST(idc.increment_value AS nvarchar(40)), ')') ELSE '' END,
            CASE WHEN c.is_computed = 1 THEN CONCAT(' AS ', cc.definition COLLATE DATABASE_DEFAULT,
                CASE WHEN cc.is_persisted = 1 THEN ' PERSISTED' ELSE '' END) ELSE '' END,
            ' COLLATE ', COALESCE(c.collation_name COLLATE DATABASE_DEFAULT, '-'),
            ' DEFAULT ', COALESCE(dc.definition COLLATE DATABASE_DEFAULT, '-')) AS Value
        FROM sys.columns c
        JOIN sys.tables t ON t.object_id = c.object_id
        JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        LEFT JOIN sys.identity_columns idc ON idc.object_id = c.object_id AND idc.column_id = c.column_id
        LEFT JOIN sys.computed_columns cc ON cc.object_id = c.object_id AND cc.column_id = c.column_id
        LEFT JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
        WHERE t.name <> '__EFMigrationsHistory'
        """;

    // Both aggregates must share one ordering. Included columns all have key_ordinal 0, so they
    // fall back to the order they were declared in.
    private const string IndexesQuery = """
        SELECT CONCAT(t.name, '.', i.name, ': ', i.type_desc COLLATE DATABASE_DEFAULT,
            CASE WHEN i.is_primary_key = 1 THEN ' PRIMARY KEY' ELSE '' END,
            CASE WHEN i.is_unique_constraint = 1 THEN ' UNIQUE CONSTRAINT' ELSE '' END,
            CASE WHEN i.is_unique = 1 THEN ' UNIQUE' ELSE '' END,
            ' (', STRING_AGG(CASE WHEN ic.is_included_column = 0
                THEN CONCAT(c.name, CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE '' END) END, ',')
                WITHIN GROUP (ORDER BY ic.key_ordinal, ic.index_column_id), ')',
            ' INCLUDE (', STRING_AGG(CASE WHEN ic.is_included_column = 1 THEN c.name END, ',')
                WITHIN GROUP (ORDER BY ic.key_ordinal, ic.index_column_id), ')',
            ' FILTER ', COALESCE(i.filter_definition COLLATE DATABASE_DEFAULT, '-')) AS Value
        FROM sys.indexes i
        JOIN sys.tables t ON t.object_id = i.object_id
        JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        WHERE t.name <> '__EFMigrationsHistory' AND i.type > 0
        GROUP BY t.name, i.name, i.type_desc, i.is_primary_key, i.is_unique_constraint, i.is_unique, i.filter_definition
        """;

    private const string ForeignKeysQuery = """
        SELECT CONCAT(OBJECT_NAME(fk.parent_object_id), '.', fk.name, ': (',
            STRING_AGG(pc.name, ',') WITHIN GROUP (ORDER BY fkc.constraint_column_id),
            ') -> ', OBJECT_NAME(fk.referenced_object_id), ' (',
            STRING_AGG(rc.name, ',') WITHIN GROUP (ORDER BY fkc.constraint_column_id),
            ') ON DELETE ', fk.delete_referential_action_desc COLLATE DATABASE_DEFAULT,
            ' ON UPDATE ', fk.update_referential_action_desc COLLATE DATABASE_DEFAULT) AS Value
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
        JOIN sys.columns rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
        GROUP BY fk.parent_object_id, fk.name, fk.referenced_object_id,
            fk.delete_referential_action_desc, fk.update_referential_action_desc
        """;

    private const string CheckConstraintsQuery = """
        SELECT CONCAT(OBJECT_NAME(ck.parent_object_id), '.', ck.name, ': ',
            ck.definition COLLATE DATABASE_DEFAULT) AS Value
        FROM sys.check_constraints ck
        """;

    private const string SequencesQuery = """
        SELECT CONCAT(s.name, ': ', TYPE_NAME(s.user_type_id),
            ' START ', CAST(s.start_value AS nvarchar(40)),
            ' INCREMENT ', CAST(s.increment AS nvarchar(40)),
            ' MIN ', CAST(s.minimum_value AS nvarchar(40)),
            ' MAX ', CAST(s.maximum_value AS nvarchar(40)),
            CASE WHEN s.is_cycling = 1 THEN ' CYCLE' ELSE '' END,
            ' CACHE ', COALESCE(CAST(s.cache_size AS nvarchar(40)), '-')) AS Value
        FROM sys.sequences s
        """;

    /// <summary>The kinds of line <see cref="DescribeAsync"/> returns, each its line prefix.</summary>
    public static readonly IReadOnlyList<string> Sections =
        ["table", "column", "index", "foreign key", "check", "sequence"];

    public static async Task<IReadOnlyList<string>> DescribeAsync(DbContext context, CancellationToken cancellationToken)
    {
        var lines = new List<string>();
        string[] queries = [TablesQuery, ColumnsQuery, IndexesQuery, ForeignKeysQuery, CheckConstraintsQuery, SequencesQuery];
        foreach (var (section, query) in Sections.Zip(queries))
        {
            var rows = await context.Database.SqlQueryRaw<string>(query).ToListAsync(cancellationToken);
            lines.AddRange(rows.ConvertAll(row => $"{section} {row}"));
        }

        lines.Sort(StringComparer.Ordinal);
        return lines;
    }
}
