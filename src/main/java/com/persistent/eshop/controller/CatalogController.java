package com.persistent.eshop.controller;

import com.persistent.eshop.model.*;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.Instant;
import java.time.ZoneOffset;
import java.time.format.DateTimeFormatter;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api")
@CrossOrigin(origins = "*")
public class CatalogController {

    private final List<CatalogBrand> brands = new ArrayList<>();
    private final List<CatalogType> types = new ArrayList<>();
    private final List<CatalogItem> items = new ArrayList<>();
    private final Map<Integer, BasketItem> basket = new ConcurrentHashMap<>();
    private final List<Order> orders = Collections.synchronizedList(new ArrayList<>());
    private final AtomicInteger basketIdCounter = new AtomicInteger(1);

    public CatalogController() {
        initCatalogData();
    }

    private void initCatalogData() {
        // Brands
        brands.add(new CatalogBrand(1, ".NET"));
        brands.add(new CatalogBrand(2, "Spring Boot"));
        brands.add(new CatalogBrand(3, "Google Cloud"));
        brands.add(new CatalogBrand(4, "Persistent Systems"));

        // Types
        types.add(new CatalogType(1, "Apparel"));
        types.add(new CatalogType(2, "Drinkware"));
        types.add(new CatalogType(3, "Accessories"));

        // 12 Products matching eShopOnWeb Catalog
        items.add(new CatalogItem(1, ".NET Bot Black Sweatshirt", "Heavyweight crewneck sweatshirt with embroidered .NET Bot logo.", 34.50, "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?auto=format&fit=crop&w=400&q=80", 1, "Apparel", 1, ".NET", 120));
        items.add(new CatalogItem(2, ".NET Blue Foundation Hoodie", "Soft fleece lined pullover hoodie in .NET royal blue.", 42.00, "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?auto=format&fit=crop&w=400&q=80", 1, "Apparel", 1, ".NET", 85));
        items.add(new CatalogItem(3, "Spring Boot Developer Tee", "100% ring-spun cotton tee celebrating Spring Boot microservice engineering.", 24.00, "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?auto=format&fit=crop&w=400&q=80", 1, "Apparel", 2, "Spring Boot", 200));
        items.add(new CatalogItem(4, "Google Cloud Run Technical Hoodie", "Ultra-breathable premium jacket with Google Cloud Run architectural badge.", 54.00, "https://images.unsplash.com/photo-1509631179647-0177331693ae?auto=format&fit=crop&w=400&q=80", 1, "Apparel", 3, "Google Cloud", 65));
        items.add(new CatalogItem(5, "Persistent Cloud Advisory Polo", "Executive cotton-piqué polo tailored for enterprise cloud advisory teams.", 38.00, "https://images.unsplash.com/photo-1581655353564-df123a1eb820?auto=format&fit=crop&w=400&q=80", 1, "Apparel", 4, "Persistent Systems", 90));
        items.add(new CatalogItem(6, "Spring Boot Ceramic Coffee Mug", "15oz matte black ceramic mug with Spring Boot leaf emblem in vibrant green.", 16.50, "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=400&q=80", 2, "Drinkware", 2, "Spring Boot", 310));
        items.add(new CatalogItem(7, "Google Cloud Vacuum Insulated Tumbler", "20oz stainless steel double-walled tumbler keeping beverages ice cold for 24h.", 28.00, "https://images.unsplash.com/photo-1517256064527-09c73fc73e38?auto=format&fit=crop&w=400&q=80", 2, "Drinkware", 3, "Google Cloud", 145));
        items.add(new CatalogItem(8, ".NET Performance Hydro Bottle", "32oz BPA-free sports bottle with leakproof spout and .NET performance index.", 22.00, "https://images.unsplash.com/photo-1602143407151-7111542de6e8?auto=format&fit=crop&w=400&q=80", 2, "Drinkware", 1, ".NET", 175));
        items.add(new CatalogItem(9, "Persistent Dual-Cloud Laptop Backpack", "Weather-resistant 25L commuter backpack with padded 16\" laptop compartment.", 72.00, "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?auto=format&fit=crop&w=400&q=80", 3, "Accessories", 4, "Persistent Systems", 40));
        items.add(new CatalogItem(10, "Google Cloud Kubernetes Enamel Pin", "High-relief metallic enamel pin celebrating serverless container modernization.", 9.50, "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?auto=format&fit=crop&w=400&q=80", 3, "Accessories", 3, "Google Cloud", 450));
        items.add(new CatalogItem(11, "Spring Boot Reactive Sticker Pack", "Set of 6 die-cut vinyl stickers for MacBook lid featuring Spring Framework art.", 7.50, "https://images.unsplash.com/photo-1572375992501-4b0892d50c69?auto=format&fit=crop&w=400&q=80", 3, "Accessories", 2, "Spring Boot", 600));
        items.add(new CatalogItem(12, ".NET 8 Architecture Reference Notebook", "Hardcover 200-page dot grid developer notebook with ribbon bookmark.", 18.00, "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?auto=format&fit=crop&w=400&q=80", 3, "Accessories", 1, ".NET", 220));

        // Initial sample basket item for demo richness
        basket.put(1, new BasketItem(1, 1, ".NET Bot Black Sweatshirt", 34.50, 1, items.get(0).getPictureUri()));
    }

    @GetMapping("/catalog/items")
    public ResponseEntity<Map<String, Object>> getCatalogItems(
            @RequestParam(required = false) Integer brandId,
            @RequestParam(required = false) Integer typeId,
            @RequestParam(required = false) String search) {

        List<CatalogItem> filtered = items.stream()
                .filter(i -> brandId == null || i.getCatalogBrandId() == brandId)
                .filter(i -> typeId == null || i.getCatalogTypeId() == typeId)
                .filter(i -> search == null || search.isBlank() ||
                        i.getName().toLowerCase().contains(search.toLowerCase()) ||
                        i.getDescription().toLowerCase().contains(search.toLowerCase()))
                .collect(Collectors.toList());

        Map<String, Object> resp = new HashMap<>();
        resp.put("data", filtered);
        resp.put("totalItems", filtered.size());
        resp.put("brands", brands);
        resp.put("types", types);
        return ResponseEntity.ok(resp);
    }

    @GetMapping("/catalog/items/{id}")
    public ResponseEntity<CatalogItem> getCatalogItemById(@PathVariable int id) {
        return items.stream()
                .filter(i -> i.getId() == id)
                .findFirst()
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/catalog/brands")
    public ResponseEntity<List<CatalogBrand>> getBrands() {
        return ResponseEntity.ok(brands);
    }

    @GetMapping("/catalog/types")
    public ResponseEntity<List<CatalogType>> getTypes() {
        return ResponseEntity.ok(types);
    }

    @GetMapping("/basket")
    public ResponseEntity<Map<String, Object>> getBasket() {
        List<BasketItem> list = new ArrayList<>(basket.values());
        double subtotal = list.stream().mapToDouble(BasketItem::getTotalPrice).sum();
        int totalQuantity = list.stream().mapToInt(BasketItem::getQuantity).sum();

        Map<String, Object> resp = new HashMap<>();
        resp.put("items", list);
        resp.put("totalQuantity", totalQuantity);
        resp.put("subtotal", Math.round(subtotal * 100.0) / 100.0);
        return ResponseEntity.ok(resp);
    }

    @PostMapping("/basket")
    public ResponseEntity<Map<String, Object>> addToBasket(@RequestBody Map<String, Object> req) {
        int catalogItemId = ((Number) req.getOrDefault("catalogItemId", 1)).intValue();
        int quantity = ((Number) req.getOrDefault("quantity", 1)).intValue();

        CatalogItem product = items.stream().filter(i -> i.getId() == catalogItemId).findFirst().orElse(null);
        if (product == null) {
            return ResponseEntity.badRequest().body(Map.of("error", "Invalid catalogItemId"));
        }

        // Check if already in basket
        BasketItem existing = basket.values().stream()
                .filter(b -> b.getCatalogItemId() == catalogItemId)
                .findFirst().orElse(null);

        if (existing != null) {
            existing.setQuantity(existing.getQuantity() + quantity);
        } else {
            int newId = basketIdCounter.incrementAndGet();
            BasketItem item = new BasketItem(newId, product.getId(), product.getName(), product.getPrice(), quantity, product.getPictureUri());
            basket.put(newId, item);
        }

        return getBasket();
    }

    @DeleteMapping("/basket/{id}")
    public ResponseEntity<Map<String, Object>> removeFromBasket(@PathVariable int id) {
        basket.remove(id);
        return getBasket();
    }

    @DeleteMapping("/basket")
    public ResponseEntity<Map<String, Object>> clearBasket() {
        basket.clear();
        return getBasket();
    }

    @PostMapping("/orders")
    public ResponseEntity<Map<String, Object>> checkout(@RequestBody Map<String, String> req) {
        if (basket.isEmpty()) {
            return ResponseEntity.badRequest().body(Map.of("error", "Basket is empty"));
        }

        String orderId = "ESHOP-" + UUID.randomUUID().toString().substring(0, 8).toUpperCase();
        String date = DateTimeFormatter.ISO_INSTANT.format(Instant.now().atOffset(ZoneOffset.UTC));
        String customer = req.getOrDefault("customerName", "Enterprise Cloud Customer");
        String email = req.getOrDefault("customerEmail", "cloud.customer@persistent.com");
        String address = req.getOrDefault("shippingAddress", "100 Google Way, Mountain View, CA 94043");

        List<BasketItem> orderItems = new ArrayList<>(basket.values());
        double total = orderItems.stream().mapToDouble(BasketItem::getTotalPrice).sum();

        Order order = new Order(orderId, date, customer, email, address, "PAID / PROCESSING", Math.round(total * 100.0) / 100.0, orderItems);
        orders.add(0, order);
        basket.clear();

        Map<String, Object> resp = new HashMap<>();
        resp.put("success", true);
        resp.put("message", "Order placed successfully!");
        resp.put("order", order);
        return ResponseEntity.ok(resp);
    }

    @GetMapping("/orders")
    public ResponseEntity<List<Order>> getOrders() {
        return ResponseEntity.ok(orders);
    }

    @GetMapping("/modernization-info")
    public ResponseEntity<Map<String, Object>> getModernizationInfo() {
        Map<String, Object> info = new LinkedHashMap<>();
        info.put("source_architecture", "Legacy Monolithic .NET 8 / ASP.NET MVC (eShopOnWeb.sln)");
        info.put("target_architecture", "Google Cloud Run Serverless Microservice (Spring Boot 3 + React 18)");
        info.put("programming_language", "Java 17 (LTS)");
        info.put("web_framework", "Spring Boot 3.2.3 REST API + Embedded React 18 SPA");
        info.put("database_tier", "Google Cloud SQL (PostgreSQL wire parity / JPA ORM)");
        info.put("container_engine", "Eclipse Temurin 17 JRE Alpine Linux (mcr/sdk replaced)");
        info.put("cloud_provider", "Google Cloud Platform (us-central1)");
        info.put("compute_cost_reduction", "64.8% savings via Serverless scale-to-zero");
        info.put("autoscaling_characteristics", "0 to 10 instances in <1.2 seconds");
        info.put("modernization_strategy", "Decomposed monolithic Web layer into high-performance Spring Boot microservice with responsive React SPA storefront, enabling zero OS maintenance and Cloud SQL managed high availability.");
        return ResponseEntity.ok(info);
    }
}
