package com.persistent.eshop.model;

public class CatalogItem {
    private int id;
    private String name;
    private String description;
    private double price;
    private String pictureUri;
    private int catalogTypeId;
    private String catalogType;
    private int catalogBrandId;
    private String catalogBrand;
    private int unitsInStock;

    public CatalogItem() {}

    public CatalogItem(int id, String name, String description, double price, String pictureUri,
                       int catalogTypeId, String catalogType, int catalogBrandId, String catalogBrand, int unitsInStock) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.pictureUri = pictureUri;
        this.catalogTypeId = catalogTypeId;
        this.catalogType = catalogType;
        this.catalogBrandId = catalogBrandId;
        this.catalogBrand = catalogBrand;
        this.unitsInStock = unitsInStock;
    }

    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public String getName() { return name; }
    public void setName(String name) { this.name = name; }

    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }

    public double getPrice() { return price; }
    public void setPrice(double price) { this.price = price; }

    public String getPictureUri() { return pictureUri; }
    public void setPictureUri(String pictureUri) { this.pictureUri = pictureUri; }

    public int getCatalogTypeId() { return catalogTypeId; }
    public void setCatalogTypeId(int catalogTypeId) { this.catalogTypeId = catalogTypeId; }

    public String getCatalogType() { return catalogType; }
    public void setCatalogType(String catalogType) { this.catalogType = catalogType; }

    public int getCatalogBrandId() { return catalogBrandId; }
    public void setCatalogBrandId(int catalogBrandId) { this.catalogBrandId = catalogBrandId; }

    public String getCatalogBrand() { return catalogBrand; }
    public void setCatalogBrand(String catalogBrand) { this.catalogBrand = catalogBrand; }

    public int getUnitsInStock() { return unitsInStock; }
    public void setUnitsInStock(int unitsInStock) { this.unitsInStock = unitsInStock; }
}
