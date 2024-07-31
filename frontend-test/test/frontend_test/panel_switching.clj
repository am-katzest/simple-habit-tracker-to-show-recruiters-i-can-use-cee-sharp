(ns frontend-test.panel-switching
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input wait-enabled wait-disabled wait-exists wait-predicate query random-str lazy-is any] :as h  :refer-macros true]
            [frontend-test.fragments :as f]
            [etaoin.api :as e]))

(deftest ^:parallel panel-switching-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (testing "data loads properly when panel switches"
       (let [username (f/new-user)]
         (testing "account"
           (testing "query changed"
             (f/goto-panel :nav-account)
             (is (= "?panel=account" (query (e/get-url)))))
           (testing "account panel contains username fresh account"
             (lazy-is (e/has-text? username)))
           (testing "account panel contains username after refresh"
             (e/refresh)
             (f/goto-panel :nav-account)
             (lazy-is (e/has-text? username))))
         (testing "habits"
           (f/goto-panel :nav-habits)
           (is (= "?panel=habits" (query (e/get-url))))
           (f/click (btn :add-new-habit))
           (f/fill (input :habit-edit-name) "meowing")
           (f/click (any :habit-edit-save))
           (testing "habits panel has habits"
             (lazy-is (e/has-text? (btn :habit-list-item-selected) "meowing")))
           (testing "habits panel has habits after refresh"
             (e/refresh)
             (lazy-is (e/has-text? (btn :habit-list-item-selected) "meowing")))
           (testing "habits panel has habits after panel change"
             (f/goto-panel :nav-account)
             (f/goto-panel :nav-habits)
             (lazy-is (e/has-text? (btn :habit-list-item-selected) "meowing")))))))))
