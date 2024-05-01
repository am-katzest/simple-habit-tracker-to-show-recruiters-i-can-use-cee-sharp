(ns test-runner
  (:require [clojure.test :as t]
            [babashka.fs :as fs]
            [clojure.string :as s]))
;; mostly copied from babashka book
(defn path->name [p]
  (let [f (-> p fs/file-name fs/split-ext first)]
    (symbol (str "tests." f))))

(def test-namespaces
  (map path->name (fs/glob "tests/" "**.bb")))
(apply require test-namespaces)

(def test-results
  (apply t/run-tests test-namespaces))

(let [{:keys [fail error]} test-results]
  (when (pos? (+ fail error))
    (System/exit 1)))
