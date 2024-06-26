#!/usr/bin/bb
(ns frontend-test.setup
  (:require
   [clojure.walk :refer [postwalk]]
   [etaoin.api :as e]))

(def ROOT (or (System/getenv "FRONTEND_TEST_URL")
              "http://localhost:8880/"))

(defn url [rest] (str ROOT [rest]))

(def DRIVER (get {"FIREFOX" e/firefox} (System/getenv "FRONTEND_TEST_WEBDRIVER") e/firefox))
(defn make-driver [] (DRIVER))
(defmacro with-driver  [name & exprs]
  `(let [~name (e/firefox)]
     (try ~@exprs
          (finally
            (e/quit ~name)))))

(defmacro with-driver-no-clutter  [first & forms]
  (let [to-replace (namespace (symbol first))
        u (gensym)
        transform (fn [form] (println form)
                    (if-not (and (seq? form) (> (count form) 0))
                      form
                      (let [[first & rest] form]
                        (if-not
                         (and (symbol? first)
                              (= to-replace (namespace (symbol first))))
                          form
                          (concat [first u] rest)))))
        updated (postwalk transform forms)]
    `(s/with-driver ~u ~@updated)))
