(ns frontend-test.habits-panel
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input wait-enabled wait-disabled wait-exists wait-predicate query random-str lazy-is any textarea] :as h  :refer-macros true]
            [frontend-test.fragments :as f]
            [etaoin.api :as e]))

(defn test-buttons-no-change []
  (s/use-driver
   e/go
   (lazy-is (e/exists? (any :habit-edit-save false)) "save disabled")
   (lazy-is (e/absent? (any :habit-edit-undo)) "no undo")
   (lazy-is (e/exists? (any :habit-edit-delete)) "delete enabled")))

(defn test-buttons-change []
  (s/use-driver
   e/go
   (lazy-is (e/exists? (any :habit-edit-save)) "save enabled")
   (lazy-is (e/exists? (any :habit-edit-undo)) "undo enabled")
   (lazy-is (e/absent? (any :habit-edit-delete)) "no delete")))

(deftest ^:parallel  habit-main-panel-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (is (some? (f/new-user)))
     (f/goto-panel :nav-habits)
     (lazy-is (e/exists? (btn :add-new-habit)) "new habit button exists")
     (testing "list starts empty"
       (is (e/absent? (any :habit-list-item)))
       (is (e/absent? (any :habit-list-item-selected))))
     (is (e/absent? (input :habit-edit-name)) "editor doesn't show")
     (f/click (btn :add-new-habit))
     (lazy-is (e/exists? (any :habit-list-item-selected)) "added to list")
     (lazy-is (e/exists? (input :habit-edit-name)) "editor shows")
     (testing "edit"
       (testing "initial"
         (test-buttons-no-change))
       (testing "after inputting name"
         (f/fill (input :habit-edit-name) "mraww")
         (test-buttons-change))
       (testing "after undo"
         (f/click (any :habit-edit-undo))
         (test-buttons-no-change)
         (is (not (e/has-text? "mraww"))))
       (testing "after messing with description"
         (f/fill (textarea :habit-edit-description) "meow")
         (test-buttons-change)
         (e/clear (textarea :habit-edit-description))
         (test-buttons-no-change))
       (testing "saving"
         (f/fill (textarea :habit-edit-description) "meow meow meow")
         (f/fill (input :habit-edit-name) "meowing")
         (f/click (any :habit-edit-save))
         (test-buttons-no-change)
         (lazy-is (e/has-text? (any :habit-list-item-selected) "meowing") "updated on list")))
     (testing "still there after refresh"
       (e/refresh)
       (lazy-is (e/has-text? (any :habit-list-item-selected) "meowing") "stil on list")
       (lazy-is (= "meowing" (e/get-element-value (input :habit-edit-name))) "in name editor box")
       (lazy-is (e/has-text? (textarea :habit-edit-description) "meow meow meow"))
       (test-buttons-no-change))
     (testing "adding another one"
       (f/click (btn :add-new-habit))
       (testing "previosu one isn't selected"
         (lazy-is (not (e/has-text? (any :habit-list-item-selected) "meowing")))
         (lazy-is (lazy-is (e/has-text? (any :habit-list-item) "meowing")))
         (lazy-is (not= "meowing" (e/get-element-value (input :habit-edit-name))) "in name editor box"))
       (testing "editing it"
         (f/fill (input :habit-edit-name) "other one")
         (f/click (any :habit-edit-save)))
       (lazy-is (e/has-text? (any :habit-list-item-selected) "other one")))
     (testing "switching habits"
       (f/click (any :habit-list-item))
       (lazy-is (e/has-text? (any :habit-list-item-selected) "meowing"))
       (lazy-is (= "meowing" (e/get-element-value (input :habit-edit-name))) "in name editor box"))
     (testing "deleting"
       (testing "popup panel shows up"
         (f/click (any :habit-edit-delete))
         (lazy-is (e/exists? (any :delete-habit-confirm-panel))))
       (testing "panel closes"
         (f/click (btn :confirm-panel-cancel))
         (lazy-is (not (e/exists? (any :delete-habit-confirm-panel)))))
       (testing "not deleted"
         (lazy-is (e/has-text? "meowing")))
       (testing "actually deleting"
         (f/click (any :habit-edit-delete))
         (f/click (any :confirm-panel-confirm))
         (lazy-is (not (e/has-text? "meowing")) "first one gone")
         (lazy-is (= "other one" (e/get-element-value (input :habit-edit-name))) "switched to other")
         (f/click (any :habit-edit-delete))
         (f/click (any :confirm-panel-confirm))
         (lazy-is (not (or (e/exists? :habit-list-item)
                           (e/exists? :habit-list-item-selected)))))))))
