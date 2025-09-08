# syntax=docker/dockerfile:1.7

# ⚠️ Node 18 is EOL (Apr 30, 2025). Prefer 20 or 22.
FROM node:20-slim AS base

# Add tini + curl for healthcheck
RUN apt-get update \
 && apt-get install -y --no-install-recommends tini curl \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
ENV NODE_ENV=production

# Install only prod deps, leveraging cache
COPY package*.json ./
RUN --mount=type=cache,target=/root/.npm \
    npm ci --omit=dev

# Copy the rest AFTER deps so code changes don’t bust the deps layer
COPY . .

# Optional: if you have a build step (e.g., React/Next)
# RUN npm run build

ENV PORT=80
EXPOSE 80

# Lightweight init to reap zombies & forward signals
ENTRYPOINT ["tini","--"]
CMD ["node","server.js"]
