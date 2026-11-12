package com.persistent.eshop.model;

public class BasketItem {
    private int id;
    private int catalogItemId;
    private String productName;
    private double unitPrice;
    private int quantity;
    private String pictureUri;

    public BasketItem() {}

    public BasketItem(int id, int catalogItemId, String productName, double unitPrice, int quantity, String pictureUri) {
        this.id = id;
        this.catalogItemId = catalogItemId;
        this.productName = productName;
        this.unitPrice = unitPrice;
        this.quantity = quantity;
        this.pictureUri = pictureUri;
    }

    public int getId() { return id; }
    public void setId(int id) { this.id = id; }

    public int getCatalogItemId() { return catalogItemId; }
    public void setCatalogItemId(int catalogItemId) { this.catalogItemId = catalogItemId; }

    public String getProductName() { return productName; }
    public void setProductName(String productName) { this.productName = productName; }

    public double getUnitPrice() { return unitPrice; }
    public void setUnitPrice(double unitPrice) { this.unitPrice = unitPrice; }

    public int getQuantity() { return quantity; }
    public void setQuantity(int quantity) { this.quantity = quantity; }

    public String getPictureUri() { return pictureUri; }
    public void setPictureUri(String pictureUri) { this.pictureUri = pictureUri; }

    public double getTotalPrice() {
        return Math.round(unitPrice * quantity * 100.0) / 100.0;
    }
}
