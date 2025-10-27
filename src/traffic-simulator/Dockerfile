# Traffic simulator container for eShopOnWeb
FROM mcr.microsoft.com/playwright:v1.56.1-jammy

WORKDIR /app

# Copy package files
COPY package.json ./

# Install dependencies
RUN npm install --production

# Copy simulation script
COPY simulate-traffic.js ./

# Environment variable (required at runtime)
ENV APP_URL=""

# Run simulation (loops forever automatically)
ENTRYPOINT ["node", "simulate-traffic.js"]

