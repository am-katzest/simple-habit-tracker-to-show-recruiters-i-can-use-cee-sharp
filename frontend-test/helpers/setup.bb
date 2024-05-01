#!/usr/bin/bb
(ns helpers.setup
  (:require
   [etaoin.api :as e]))

(def ROOT (or (System/getenv "FRONTEND_TEST_URL")
              "http://localhost:8280/"))
(defn make-driver [] (e/firefox))
(defmacro with-driver  [name & exprs]
  `(let [~name (e/firefox)]
     (try ~@exprs
          (finally
            (e/quit ~name)))))
