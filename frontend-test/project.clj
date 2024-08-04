(defproject frontend-test "0.1.0-SNAPSHOT"
  :description "tests for frontend"
  :dependencies [[org.clojure/clojure "1.11.1"]
                 [etaoin/etaoin "1.0.40"]]
  :plugins [[com.holychao/parallel-test "0.3.2"]]
  :repl-options {:init-ns frontend-test.core})
