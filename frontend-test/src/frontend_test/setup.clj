#!/usr/bin/bb
(ns frontend-test.setup
  (:require
   [etaoin.api :as e]))

(def ROOT (or (System/getenv "FRONTEND_TEST_URL")
              "http://localhost:8880/"))

(def DRIVER (get {"FIREFOX" e/firefox} (System/getenv "FRONTEND_TEST_WEBDRIVER")
                 "http://localhost:8280/"))
(defn make-driver [] (DRIVER))
(defmacro with-driver  [name & exprs]
  `(let [~name (e/firefox)]
     (try ~@exprs
          (finally
            (e/quit ~name)))))
