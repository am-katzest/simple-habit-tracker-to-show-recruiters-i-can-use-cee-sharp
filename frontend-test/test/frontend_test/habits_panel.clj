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
         (absent? :habit-list-item)
         (absent? :habit-list-item-selected))))))

(deftest ^:parallel habit-ct-test
  ;; can't test color picker because
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (testing "setup"
       (is (some? (f/new-user)))
       (f/goto-panel :nav-habits)
       (doseq [habit ["habit_a" "habit_b"]]
         (f/click :add-new-habit)
         (f/fill :habit-edit-name habit)
         (f/click :habit-edit-save)))
     (f/click :habit-tab-cts)
     (testing "add button shows"
       (exists? (btn :add-new-ct)))
     (testing "list starts empty"
       (absent? :ct-list-item-selected)
       (absent? :ct-list-item))
     (testing "editing"
       (absent? :ct-edit-name "editor doesn't show"))
     (f/click :add-new-ct)
     (f/click :add-new-ct)
     (exists? :ct-list-item-selected "added to list")
     (lazy-is (e/exists? (input :ct-edit-name)) "editor shows")
     (testing "edit"
       (testing "initial"
         (test-buttons-no-change :ct-edit))
       (testing "empty name input"
         (f/fill :ct-edit-name "   ")
         (exists? (any :ct-edit-save false) "unable to save")
         (f/click :ct-edit-undo))
       (testing "unchanged name"
         (f/fill :ct-edit-name "meow")
         (f/click :ct-edit-save)
         (testing "identical"
           (f/fill :ct-edit-name "meow")
           (exists? (any :ct-edit-save false) "unable to save"))
         (testing "with added space"
           (f/fill :ct-edit-name "meow   ")
           (exists? (any :ct-edit-save false) "unable to save")))
       (testing "description"
         (testing "unchanged"
           (f/fill :ct-edit-description "")
           (exists? (any :ct-edit-save false) "unable to save"))
         (f/fill :ct-edit-description "some description")
         (f/click :ct-edit-save))
       (testing "there is a color picker"
         (exists? (input :ct-edit-color))))
     (testing "other ct unaffected"
       (f/click :ct-list-item)
       (lazy-is (not= "meow" (e/get-element-value (input :ct-edit-name)))))
     (testing "changes remain after return"
       (f/click :ct-list-item)
       (lazy-is (= "meow" (e/get-element-value (input :ct-edit-name)))))
     (testing "changes remain after refresh"
       (e/refresh)
       (f/click (h/list-item "habit_b"))
       (f/click :habit-tab-cts)
       (exists? {:fn/text "meow"})
       (f/click (h/list-item "meow"))
       (lazy-is (= "meow" (e/get-element-value (input :ct-edit-name))))
       (lazy-is (= "some description" (e/get-element-value (textarea :ct-edit-description)))))
     (testing "other habit is empty"
       (f/click (h/list-item "habit_a"))
       (f/click :habit-tab-cts)
       (exists? :add-new-ct)
       (absent? :ct-list-item)
       (absent? :ct-list-item-selected))
     (testing "deleting"
       (f/click (h/list-item "habit_b"))
       (f/click :habit-tab-cts)
       (f/click (h/list-item "meow"))
       (f/click :ct-list-item) ;;select the other one
       (f/click :ct-edit-delete)
       (f/click :confirm-panel-confirm)
       ;; (f/click {:fn/text "meow" :tag :label})
       (absent? :ct-list-item)
       (exists? :ct-list-item-selected)
       (lazy-is (= "meow" (e/get-element-value (input :ct-edit-name))))
       (f/click :ct-edit-delete)
       (f/click :confirm-panel-confirm)
       (absent? :ct-list-item)
       (absent? :ct-list-item-selected)))))
