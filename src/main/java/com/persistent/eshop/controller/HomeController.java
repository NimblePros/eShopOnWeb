package com.persistent.eshop.controller;

import org.springframework.core.io.ClassPathResource;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.util.StreamUtils;
import org.springframework.web.bind.annotation.GetMapping;

import java.io.IOException;
import java.nio.charset.StandardCharsets;

@Controller
public class HomeController {

    private String cachedIndexHtml = null;

    @GetMapping(value = {"/", "/index.html", "/store"}, produces = MediaType.TEXT_HTML_VALUE)
    public ResponseEntity<String> index() {
        try {
            if (cachedIndexHtml == null) {
                ClassPathResource resource = new ClassPathResource("static/index.html");
                cachedIndexHtml = StreamUtils.copyToString(resource.getInputStream(), StandardCharsets.UTF_8);
            }
            return ResponseEntity.ok()
                    .contentType(MediaType.TEXT_HTML)
                    .body(cachedIndexHtml);
        } catch (IOException e) {
            return ResponseEntity.status(500).body("<h1>Error loading eShopOnWeb Storefront</h1><p>" + e.getMessage() + "</p>");
        }
    }
}
