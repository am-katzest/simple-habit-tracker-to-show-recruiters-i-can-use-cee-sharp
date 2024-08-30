# simple habit tracker



# gui developement

1. start backend (and frontend that's going to be replaced)

```sh
docker-compose up -d --build
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

# gui tests

## outside developement
``` sh
./run-integration-tests.sh
```
## during developement
```sh 
cd frontend-test
lein test
```

`
