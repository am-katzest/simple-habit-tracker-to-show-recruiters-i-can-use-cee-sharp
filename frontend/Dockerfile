# huge thing
FROM strategicblue/cljs-builder as build
RUN mkdir -p /opt/build/
WORKDIR /opt/build
RUN \
    --mount=type=cache,target=/opt/build/node_modules \
    npm install react
COPY ./package.json /opt/build/
COPY ./package-lock.json /opt/build/
RUN \
    --mount=type=cache,target=/opt/build/node_modules \
    npm install
COPY ./ /opt/build/
RUN \
    --mount=type=cache,target=/opt/build/node_modules \
    --mount=type=cache,target=/root/.m2 \
    --mount=type=cache,target=/opt/build/.shadow-cljs \
    npm run release
# thank you shadow-cljs for returning 0 even if compilation failed
RUN test -e resources/public/js/compiled/app.js
ENTRYPOINT ["sleep", "infinity"]
