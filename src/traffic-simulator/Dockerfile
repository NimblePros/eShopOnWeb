# Traffic simulator container for eShopOnWeb
FROM node:18-slim

WORKDIR /app

# Install system dependencies for Chromium only
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    ca-certificates \
    fonts-liberation \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libatspi2.0-0 \
    libcairo2 \
    libcups2 \
    libdbus-1-3 \
    libdrm2 \
    libgbm1 \
    libglib2.0-0 \
    libnspr4 \
    libnss3 \
    libpango-1.0-0 \
    libx11-6 \
    libxcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxkbcommon0 \
    libxrandr2 \
    xdg-utils \
    && rm -rf /var/lib/apt/lists/*

# Copy package files and install Node.js dependencies
COPY package.json ./
RUN npm install --production

# Install only Chromium browser (not all browsers)
RUN npx playwright install chromium --with-deps

# Copy simulation script
COPY simulate-traffic.js ./

# Environment variable (required at runtime)
ENV APP_URL=""

# Run simulation (loops forever automatically)
ENTRYPOINT ["node", "simulate-traffic.js"]

