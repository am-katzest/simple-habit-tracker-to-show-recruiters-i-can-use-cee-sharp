# huge thing
FROM strategicblue/cljs-builder as build
RUN mkdir -p /opt/build
WORKDIR /opt/build
#COPY ./package.json /opt/build/
#COPY ./package-lock.json /opt/build/
#RUN npm install
COPY ./ /opt/build
RUN --mount=type=cache,target=/opt/build/node_modules \
    npm install \
    npm run release
