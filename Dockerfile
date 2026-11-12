# Enterprise Multi-Stage Dockerfile for Java / Spring Boot (eShopOnWeb)
# Modernized for Google Cloud Run
FROM maven:3.9-eclipse-temurin-17-alpine AS builder
WORKDIR /app
COPY pom.xml .
COPY src ./src
RUN mvn clean package -DskipTests

FROM eclipse-temurin:17-jre-alpine AS runner
WORKDIR /app
ENV PORT=8080
EXPOSE 8080
COPY --from=builder /app/target/*.jar app.jar
USER 1001
CMD ["java", "-Dserver.port=${PORT}", "-jar", "app.jar"]
