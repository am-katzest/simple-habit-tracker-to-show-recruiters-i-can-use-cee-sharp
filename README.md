# simple habit tracker


# tests

``` sh
dotnet test
```

# gui developement

1. start backend

```
docker-compose build
docker-compose up
```
2. start relay

```
bash start_nginx_dev.sh
```
3. start watcher for updates to frontend code
```
cd frontend
npx shadow-cljs  watch app
```

# habit

user, name, description (rarely)

# result

habit, color, name, description (sometimes)

# completion

result, date, description (rarely)
