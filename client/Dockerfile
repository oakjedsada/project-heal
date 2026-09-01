# Build context is this client/ folder: docker build -f client/Dockerfile client/

FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Baked in at build time, not runtime — Vite inlines VITE_* vars into the
# compiled JS. Only needed when the API lives on a different origin (a real
# deploy); local dev never sets this (see src/api/client.ts).
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM nginx:1.27-alpine AS runtime

# Official nginx image templating: files in /etc/nginx/templates/*.template
# get envsubst'd into /etc/nginx/conf.d/*.conf at container start. Restrict
# substitution to PORT only, so nginx's own $uri/$host variables in the
# template aren't mangled by envsubst.
ENV PORT=80
ENV NGINX_ENVSUBST_FILTER=PORT
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
