#!/usr/bin/env node
/**
 * Automated traffic simulator for eShopOnWeb
 * Generates continuous realistic user traffic for Datadog monitoring
 */

const { chromium } = require('playwright');

// Configuration
const APP_URL = process.env.APP_URL;
const ITERATIONS_PER_CYCLE = 5;
const DELAY_BETWEEN_CYCLES = 60000; // 60 seconds

if (!APP_URL) {
  console.error('❌ ERROR: APP_URL environment variable is required');
  process.exit(1);
}

console.log('🎭 eShopOnWeb Traffic Simulator');
console.log(`📍 Target: ${APP_URL}`);
console.log(`🔁 Running continuously (${ITERATIONS_PER_CYCLE} sessions per cycle, 60s delay)`);
console.log('');

// Random delay to simulate human behavior
const randomDelay = (min = 500, max = 2000) => 
  new Promise(resolve => setTimeout(resolve, Math.floor(Math.random() * (max - min + 1)) + min));

// User scenarios
const scenarios = [
  {
    name: 'Browse and Add to Cart',
    weight: 50,
    async execute(page) {
      console.log('  📋 Browsing catalog...');
      await page.goto(APP_URL);
      await randomDelay(1000, 2000);

      // Browse catalog
      console.log('  🔍 Filtering by brand...');
      const brandFilter = page.locator('form[method="get"] select[name="BrandFilterApplied"]').first();
      if (await brandFilter.count() > 0) {
        await brandFilter.selectOption({ index: 1 });
        await page.locator('input[type="submit"][value="Filter"]').first().click();
        await randomDelay(800, 1500);
      }

      // View product details
      console.log('  👁️  Viewing product details...');
      const productLinks = page.locator('a:has-text("Details")');
      const count = await productLinks.count();
      if (count > 0) {
        await productLinks.nth(Math.floor(Math.random() * count)).click();
        await randomDelay(1000, 2000);

        // Add to cart
        console.log('  🛒 Adding to cart...');
        const addToCartBtn = page.locator('input[type="submit"][value="ADD TO BASKET"]');
        if (await addToCartBtn.count() > 0) {
          await addToCartBtn.click();
          await randomDelay(500, 1000);
        }
      }

      // View basket
      console.log('  🛍️  Viewing basket...');
      await page.goto(`${APP_URL}/basket`);
      await randomDelay(1000, 1500);
    }
  },
  {
    name: 'Quick Browse',
    weight: 30,
    async execute(page) {
      console.log('  🏠 Visiting homepage...');
      await page.goto(APP_URL);
      await randomDelay(1000, 2000);

      // Browse a few products
      console.log('  👀 Browsing products...');
      const productLinks = page.locator('a:has-text("Details")');
      const count = await productLinks.count();
      if (count > 0) {
        const viewCount = Math.min(3, count);
        for (let i = 0; i < viewCount; i++) {
          await page.goto(APP_URL);
          await randomDelay(500, 1000);
          await productLinks.nth(Math.floor(Math.random() * count)).click();
          await randomDelay(800, 1500);
        }
      }
    }
  },
  {
    name: 'Admin Panel Visit',
    weight: 20,
    async execute(page) {
      console.log('  🔐 Checking admin panel...');
      await page.goto(`${APP_URL}/admin`);
      await randomDelay(1000, 2000);
      
      // Try to access (will redirect to login if not authenticated)
      console.log('  📊 Admin page loaded');
    }
  }
];

// Select scenario based on weights
function selectScenario() {
  const totalWeight = scenarios.reduce((sum, s) => sum + s.weight, 0);
  let random = Math.random() * totalWeight;
  
  for (const scenario of scenarios) {
    random -= scenario.weight;
    if (random <= 0) return scenario;
  }
  
  return scenarios[0];
}

async function runSimulation() {
  const startTime = Date.now();
  let successCount = 0;
  let errorCount = 0;

  const browser = await chromium.launch({ headless: true });

  for (let i = 1; i <= ITERATIONS_PER_CYCLE; i++) {
    const scenario = selectScenario();
    console.log(`\n[${i}/${ITERATIONS_PER_CYCLE}] Running: ${scenario.name}`);
    
    const context = await browser.newContext({
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
      viewport: { width: 1920, height: 1080 }
    });
    const page = await context.newPage();

    try {
      await scenario.execute(page);
      console.log(`  ✅ Completed successfully`);
      successCount++;
    } catch (error) {
      console.error(`  ❌ Error: ${error.message}`);
      errorCount++;
    } finally {
      await context.close();
      await randomDelay(2000, 4000); // Delay between sessions
    }
  }

  await browser.close();

  const elapsed = Math.round((Date.now() - startTime) / 1000);
  console.log('\n' + '='.repeat(50));
  console.log('📊 Cycle Summary');
  console.log('='.repeat(50));
  console.log(`✅ Successful: ${successCount}/${ITERATIONS_PER_CYCLE}`);
  console.log(`❌ Errors: ${errorCount}/${ITERATIONS_PER_CYCLE}`);
  console.log(`⏱️  Duration: ${elapsed}s`);
}

// Main loop - run forever
async function main() {
  let cycleCount = 0;
  
  while (true) {
    cycleCount++;
    console.log(`\n${'='.repeat(60)}`);
    console.log(`🔄 Cycle #${cycleCount} - ${new Date().toISOString()}`);
    console.log('='.repeat(60));
    
    try {
      await runSimulation();
    } catch (error) {
      console.error('💥 Error in simulation cycle:', error.message);
    }
    
    console.log(`\n⏳ Waiting 60s before next cycle...`);
    await new Promise(resolve => setTimeout(resolve, DELAY_BETWEEN_CYCLES));
  }
}

// Run
main().catch(error => {
  console.error('💥 Fatal error:', error);
  process.exit(1);
});

