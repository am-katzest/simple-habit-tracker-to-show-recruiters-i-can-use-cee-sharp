(ns frontend-test.habits-panel
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input exists? absent? lazy-is any textarea] :as h  :refer-macros true]
            [frontend-test.fragments :as f]
            [etaoin.api :as e]))

(defn test-buttons-no-change [base]
  (s/use-driver
   e/go
   (exists? (h/join base :save)  "save enabled")
   (absent? (h/join base :undo) "no undo")
   (exists? (h/join base :delete) "delete enabled")))

(defn test-buttons-change [base]
  (s/use-driver
   e/go
   (exists? (h/join base :save)  "save enabled")
   (exists? (h/join base :undo) "undo enabled")
   (absent? (h/join base :delete) "no delete")))

(deftest ^:parallel  habit-main-panel-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (is (some? (f/new-user)))
     (f/goto-panel :nav-habits)
     (exists? (btn :add-new-habit) "new habit button exists")
     (testing "list starts empty"
       (absent? :habit-list-item)
       (absent? :habit-list-item)
       (absent? :habit-list-item-selected))
     (absent? (input :habit-edit-name) "editor doesn't show")
     (f/click (btn :add-new-habit))
     (exists? :habit-list-item-selected "added to list")
     (exists? (input :habit-edit-name) "editor shows")
     (testing "edit"
       (testing "initial"
         (test-buttons-no-change :habit-edit))
       (testing "after inputting name"
         (f/fill (input :habit-edit-name) "mraww")
         (test-buttons-change :habit-edit))
       (testing "after undo"
         (f/click :habit-edit-undo)
         (test-buttons-no-change :habit-edit)
         (is (not (e/has-text? "mraww"))))
       (testing "after messing with description"
         (f/fill (textarea :habit-edit-description) "meow")
         (test-buttons-change :habit-edit)
         (e/clear (textarea :habit-edit-description))
         (test-buttons-no-change :habit-edit))
       (testing "saving"
         (f/fill (textarea :habit-edit-description) "meow meow meow")
         (f/fill (input :habit-edit-name) "meowing")
         (f/click :habit-edit-save)
         (test-buttons-no-change :habit-edit)
         (lazy-is (e/has-text? (any :habit-list-item-selected) "meowing") "updated on list")))
     (testing "still there after refresh"
       (e/refresh)
       (lazy-is (e/has-text? (any :habit-list-item-selected) "meowing") "stil on list")
       (lazy-is (= "meowing" (e/get-element-value (input :habit-edit-name))) "in name editor box")
       (lazy-is (e/has-text? (textarea :habit-edit-description) "meow meow meow"))
       (test-buttons-no-change :habit-edit))
     (testing "adding another one"
       (f/click (btn :add-new-habit))
       (testing "previosu one isn't selected"
         (lazy-is (not (e/has-text? (any :habit-list-item-selected) "meowing")))
         (lazy-is (lazy-is (e/has-text? (any :habit-list-item) "meowing")))
         (lazy-is (not= "meowing" (e/get-element-value (input :habit-edit-name))) "in name editor box"))
       (testing "editing it"
         (f/fill (input :habit-edit-name) "other one")
         (f/click :habit-edit-save))
       (lazy-is (e/has-text? (any :habit-list-item-selected) "other one")))
     (testing "switching habits"
       (f/click :habit-list-item)
       (lazy-is (e/has-text? (any :habit-list-item-selected) "meowing"))
       (lazy-is (= "meowing" (e/get-element-value (input :habit-edit-name))) "in name editor box"))
     (testing "deleting"
       (testing "popup panel shows up"
         (f/click :habit-edit-delete)
         (exists? :habit-edit-confirm-delete))
       (testing "panel closes"
         (f/click (btn :confirm-panel-cancel))
         (absent? :habit-edit-confirm-delete))
       (testing "not deleted"
         (lazy-is (e/has-text? "meowing")))
       (testing "actually deleting"
         (f/click :habit-edit-delete)
         (f/click :confirm-panel-confirm)
         (lazy-is (not (e/has-text? "meowing")) "first one gone")
         (lazy-is (= "other one" (e/get-element-value (input :habit-edit-name))) "switched to other")
         (f/click :habit-edit-delete)
         (f/click :confirm-panel-confirm)
         (lazy-is (not (or (e/exists? :habit-list-item)
                           (e/exists? :habit-list-item-selected)))))))))
