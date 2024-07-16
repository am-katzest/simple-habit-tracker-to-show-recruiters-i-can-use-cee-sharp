FROM ryukzak/bb-with-geckodriver:latest
RUN mkdir -p /opt/tests
COPY ./project.clj /opt/tests/
WORKDIR /opt/tests
RUN lein install
COPY ./ /opt/tests/
ENV FRONTEND_TEST_WEBDRIVER FIREFOX_HEADLESS
CMD lein test
