FROM clojure:temurin-8-lein-2.11.2-alpine as CLJ
RUN mkdir -p /opt/tests
COPY ./project.clj /opt/tests/
WORKDIR /opt/tests
RUN lein install
COPY ./ /opt/tests/
CMD lein test
