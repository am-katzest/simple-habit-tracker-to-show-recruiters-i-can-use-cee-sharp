#!/usr/bin/bb
(ns frontend-test.setup
  (:require
   [clojure.walk :refer [postwalk]]
   [etaoin.api :as e]))

(def ROOT (or (System/getenv "FRONTEND_TEST_URL")
              "http://localhost:8880/"))

(defn url [rest] (str ROOT [rest]))

(def make-driver
  (get {"FIREFOX" e/firefox
        "FIREFOX_HEADLESS" e/firefox-headless}
       (System/getenv "FRONTEND_TEST_WEBDRIVER")
       e/firefox))

(def ^:dynamic *driver* nil)
(defmacro with-driver  [& exprs]
  `(let [name# (make-driver)]
     (binding [*driver* name#]
       (try ~@exprs
            (finally
              (e/quit name#))))))

(defmacro use-driver  [first & forms]
  (let [to-replace (namespace (symbol first))
        transform (fn [form]
                    (if-not (and (seq? form) (> (count form) 0))
                      form
                      (let [[first & rest] form]
                        (if-not
                         (and (symbol? first)
                              (= to-replace (namespace (symbol first))))
                          form
                          (concat [first `*driver*] rest)))))]
    `(do ~@(postwalk transform forms))))

(defmacro use-new-driver [& exprs]
  `(with-driver
     (use-driver ~@exprs)))
