# Containers

## Backend

Backend Contains compiled asp.net app from `backend/`, listens on port `8680`

## Frontend

Frontend is an Nginx container serving compiled to js & html `frontend/`, listens on port `8580`

## How it's all connected

```mermaid
C4Container
title container diagram
Container_Boundary(c1, "docker compose core, common for all modes") {
  Container_Boundary(c_backend, "backend docker container"){
    Container(api, "backend", "C#, ASP.NET", "api")
  }
  Container_Boundary(c_db, "db docker container"){
    ContainerDb(db, "database", "postgresql", "contains user data")
  }
  Container_Boundary(c_frontend, "frontend docker container"){
    Container(nginx, "reverse proxy", "nginx")
    Container(files, "frontend", "ClojureScript, re frame", "located at /var/www/mnt")
  }
  Rel(nginx, api, "reverse proxies request to /api" "http, 8680")
  Rel(nginx, files, "serves when request path isn't /api")
  Rel(api, db, "stores data at", "5432")
  UpdateRelStyle(nginx, files, $offsetY="40", $offsetX="-190")
  UpdateRelStyle(api, db, $offsetX="-40", $offsetY="-30")
}
System_Ext(external_proxy, "some external revrse proxy providing encryption")
Rel(external_proxy, nginx, "uses", "http, 8580")
UpdateRelStyle(external_proxy, nginx, $offsetY="-300")

Boundary(c_developement, "only during developement"){
  Container(shadow, "frontend watcher", "npx shadow-cljs watch app", "serves files and reloads browser when code changes")
  Container_Boundary(c_nginx_dev, "nginx_dev docker container"){
    System(nginx_dev, "reverse proxy", "nginx", "redirects")
  }
  Rel(nginx_dev, shadow, "reverse proxies paths other than /api", "http, 8280")
  Rel(nginx_dev, nginx, "reverse proxies /api", "http, 8580")
  Person(browser_dev, "web browser, during developement")
  Rel(browser_dev, nginx_dev, "uses", "http, 8880")
  UpdateRelStyle(browser_dev, nginx_dev, $offsetX="-40", $offsetY="-40")
}

Boundary(c_testt, "only during gui tests"){
  Container_Boundary(c_test, "frontend_test docker container"){
    Container(frontend_test, "frontend tests", "clojure, etaoin", "runs gui tests")
    Container(geckodriver, "browser webdriver", "geckodriver")
  }
  Rel(frontend_test, geckodriver, "Uses", "webdriver protocol")
  Rel(geckodriver, nginx, "Uses during CI", "http, 8580")
  Rel(geckodriver, nginx_dev, "Uses during developement", "http, 8880")
}
Person(browser, "web browser")
Rel(browser, external_proxy, "uses", "https, 443")
```
