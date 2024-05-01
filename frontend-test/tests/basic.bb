#!/usr/bin/bb
(ns tests.basic
  (:require [clojure.test :refer [deftest testing is are]]
            [helpers.setup :as s :refer-macros true]
            [etaoin.api :as e]))

(deftest testing-tests
  (s/with-driver -
    (doto -
      (e/go s/ROOT)
      (e/wait 1))
    (is (e/has-text? - "bad"))))
