(ns frontend-test.about
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input exists? absent? lazy-is any textarea] :as h  :refer-macros true]
            [frontend-test.fragments :as f]
            [etaoin.api :as e]))

(deftest ^:parallel  about-popup-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (is (some? (f/new-user)) "setup")
     (exists? (btn :nav-about) "button exists")
     (lazy-is (not (e/has-text? "AGPL")) "popup doesn't show before clicking")
     (f/click (btn :nav-about))
     (lazy-is (e/has-text? "AGPL") "popup shows after clicking")
     (f/click (btn :about-close))
     (lazy-is (not (e/has-text? "AGPL")) "popup closes"))))
