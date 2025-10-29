#!/usr/bin/env node
/**
 * Automated traffic simulator for eShopOnWeb
 * Generates continuous realistic user traffic for Datadog monitoring
 */

const { chromium } = require('playwright');
const winston = require('winston');

// Configure Winston with JSON logging
const logger = winston.createLogger({
  level: 'info',
  format: winston.format.json(),
  transports: [
    new winston.transports.Console()
  ]
});

// Configuration
const APP_URL = process.env.APP_URL;
const ITERATIONS_PER_CYCLE = 5;
const DELAY_BETWEEN_CYCLES = 60000; // 60 seconds

if (!APP_URL) {
  logger.error('APP_URL environment variable is required');
  process.exit(1);
}

logger.info('eShopOnWeb Traffic Simulator started', {
  target: APP_URL,
  iterationsPerCycle: ITERATIONS_PER_CYCLE,
  delayBetweenCycles: DELAY_BETWEEN_CYCLES / 1000
});

// Random delay to simulate human behavior
const randomDelay = (min = 500, max = 2000) => 
  new Promise(resolve => setTimeout(resolve, Math.floor(Math.random() * (max - min + 1)) + min));

// User scenarios
const scenarios = [
  {
    name: 'Browse and Add to Cart',
    weight: 30,
    async execute(page) {
      await page.goto(APP_URL);
      await randomDelay(1000, 2000);

      // Browse catalog
      const brandFilter = page.locator('form[method="get"] select[name="BrandFilterApplied"]').first();
      if (await brandFilter.count() > 0) {
        await brandFilter.selectOption({ index: 1 });
        await page.locator('input[type="submit"][value="Filter"]').first().click();
        await randomDelay(800, 1500);
      }

      // View product details
      const productLinks = page.locator('a:has-text("Details")');
      const count = await productLinks.count();
      if (count > 0) {
        await productLinks.nth(Math.floor(Math.random() * count)).click();
        await randomDelay(1000, 2000);

        // Add to cart
        const addToCartBtn = page.locator('input[type="submit"][value="ADD TO BASKET"]');
        if (await addToCartBtn.count() > 0) {
          await addToCartBtn.click();
          await randomDelay(500, 1000);
        }
      }

      // View basket
      await page.goto(`${APP_URL}/basket`);
      await randomDelay(1000, 1500);
    }
  },
  {
    name: 'Full Checkout Flow (Login → Add to Cart → Checkout)',
    weight: 25,
    async execute(page) {
      // 1. Login
      await page.goto(`${APP_URL}/Identity/Account/Login`);
      await randomDelay(1000, 1500);
      
      // Fill in login credentials
      await page.fill('input[name="Input.Email"]', 'demouser@microsoft.com');
      await randomDelay(300, 500);
      await page.fill('input[name="Input.Password"]', 'Pass@word1');
      await randomDelay(300, 500);
      
      await page.locator('button[type="submit"]:has-text("Log in")').click();
      await randomDelay(1500, 2000);

      // 2. Browse and add to cart
      await page.goto(APP_URL);
      await randomDelay(800, 1200);

      const productLinks = page.locator('a:has-text("Details")');
      const count = await productLinks.count();
      if (count > 0) {
        // Add 2-3 items to cart
        const itemsToAdd = Math.min(Math.floor(Math.random() * 2) + 2, count);
        for (let i = 0; i < itemsToAdd; i++) {
          await page.goto(APP_URL);
          await randomDelay(500, 800);
          await productLinks.nth(Math.floor(Math.random() * count)).click();
          await randomDelay(800, 1200);
          
          const addToCartBtn = page.locator('input[type="submit"][value="ADD TO BASKET"]');
          if (await addToCartBtn.count() > 0) {
            await addToCartBtn.click();
            await randomDelay(500, 800);
          }
        }
      }

      // 3. View basket
      await page.goto(`${APP_URL}/basket`);
      await randomDelay(1000, 1500);

      // 4. Checkout
      const checkoutBtn = page.locator('input[type="submit"][value="[ CHECKOUT ]"]');
      if (await checkoutBtn.count() > 0) {
        await checkoutBtn.click();
        await randomDelay(1500, 2000);
      }

      // 5. Logout
      await page.goto(`${APP_URL}/Identity/Account/Logout`);
      await randomDelay(500, 800);
      const logoutBtn = page.locator('button[type="submit"]:has-text("Click here to Logout")');
      if (await logoutBtn.count() > 0) {
        await logoutBtn.click();
        await randomDelay(500, 800);
      }
    }
  },
  {
    name: 'Quick Browse',
    weight: 20,
    async execute(page) {
      await page.goto(APP_URL);
      await randomDelay(1000, 2000);

      // Browse a few products
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
    name: 'API Health Check (Web → PublicApi)',
    weight: 15,
    async execute(page) {
      // This triggers a distributed trace: Web calls PublicApi
      await page.goto(`${APP_URL}/api_health_check`);
      await randomDelay(500, 1000);
    }
  },
  {
    name: 'Direct API Call (PublicApi)',
    weight: 10,
    async execute(page) {
      // Direct call to PublicApi service
      const apiUrl = APP_URL.replace('eshopwebmvc', 'eshoppublicapi');
      await page.goto(`${apiUrl}/api/catalog-items`);
      await randomDelay(500, 1000);
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
    logger.info('Running scenario', {
      iteration: i,
      total: ITERATIONS_PER_CYCLE,
      scenario: scenario.name
    });
    
    const context = await browser.newContext({
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
      viewport: { width: 1920, height: 1080 }
    });
    const page = await context.newPage();

    try {
      await scenario.execute(page);
      logger.info('Scenario completed successfully', {
        scenario: scenario.name,
        iteration: i
      });
      successCount++;
    } catch (error) {
      logger.error('Scenario failed', {
        scenario: scenario.name,
        iteration: i,
        error: error.message
      });
      errorCount++;
    } finally {
      await context.close();
      await randomDelay(2000, 4000); // Delay between sessions
    }
  }

  await browser.close();

  const elapsed = Math.round((Date.now() - startTime) / 1000);
  logger.info('Cycle summary', {
    successful: successCount,
    errors: errorCount,
    total: ITERATIONS_PER_CYCLE,
    durationSeconds: elapsed
  });
}

// Main loop - run forever
async function main() {
  let cycleCount = 0;
  
  while (true) {
    cycleCount++;
    logger.info('Starting cycle', {
      cycle: cycleCount,
      timestamp: new Date().toISOString()
    });
    
    try {
      await runSimulation();
    } catch (error) {
      logger.error('Error in simulation cycle', {
        cycle: cycleCount,
        error: error.message
      });
    }
    
    logger.info('Waiting before next cycle', {
      delaySeconds: DELAY_BETWEEN_CYCLES / 1000
    });
    await new Promise(resolve => setTimeout(resolve, DELAY_BETWEEN_CYCLES));
  }
}

// Run
main().catch(error => {
  logger.error('Fatal error', {
    error: error.message,
    stack: error.stack
  });
  process.exit(1);
});

