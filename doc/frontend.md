# Frontend

Frontend is a simple, reactive `re-frame` app

## Error Handling

when api call returns error it displays appropriate message and tries its best to reload stale data without failing.

## Developement

1. start backend (and frontend that's going to be replaced)

```sh
docker compose up -d --build
```
2. start relay

```sh
./start_nginx_dev.sh
```
3. start watcher for updates to frontend code
```sh
cd frontend
npx shadow-cljs watch app
```
4. point browser to `http://localhost:8880/`

## Tests

integration tests are implemented using `etaoin`, run on `geckodriver`

### Running Tests

there's a helper script `run-integration-tests.sh` which runs tests against app in docker-compose

during development executing 
```sh 
cd frontend-test
lein test
```
will run tests against "local" version of app (requires `geckodriver` & `leiningen` installed locally)

