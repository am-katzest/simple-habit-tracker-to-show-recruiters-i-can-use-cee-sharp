(ns frontend-test.completion
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input exists? absent? lazy-is any textarea] :as h  :refer-macros true]
            [frontend-test.fragments :as f]
            [etaoin.api :as e]))

(deftest ^:parallel  completion-adding-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (testing "setup"
       (is (some? (f/new-user)))
       (f/goto-panel :nav-habits)
       (f/click (btn :add-new-habit))
       (f/add-category ["category1"]))
     (testing "adding"
       (testing "adding fail"
         (f/click :add-new-completion-button)
         (exists? (btn :completion-edit-confirm false) "button disabled before selecting date")
         (testing "entering / exiting advanced datepicker does not set date"
           (f/click (btn :simple-datepicker-pick))
           (f/click (btn :advanced-datepicker-cancel))
           (exists? (btn :completion-edit-confirm false) "button disabled before selecting date"))
         (f/click :completion-edit-cancel))
       (testing "adding now no category"
         (f/add-completion ["item1" "now"]))
       (testing "adding today with category"
         (f/add-completion ["item2" "today" "category1"]))
       (testing "adding yesterday"
         (f/add-completion ["item3" "yesterday"]))
       ;; i don't want to mess with dates too much (beyond today / not today)
       ;; because calendar seems like a pain to use programmatically
       ;; and having tests fail on 1st of the month for no particular reason sounds like a nightmare
       ;; now only midnight is problematic
       (testing "adding date pick no time"
         (f/add-completion
          ["item4" nil nil
           (fn []
             (f/click (btn :simple-datepicker-pick))
             (f/click (btn :advanced-datepicker-confirm)))]))
       (testing "adding date pick time"
         (f/add-completion
          ["item5" nil nil
           (fn []
             (f/click (btn :simple-datepicker-pick))
             (f/fill (any :advanced-datepicker-time-input) "11:11")
             (f/click (btn :advanced-datepicker-confirm)))])))
     (testing "seeing what was added"
       (f/click :habit-tab-completions)
       (testing "todays are shown, yesterdays arent"
         (exists? {:fn/text "item1"})
         (exists? {:fn/text "item2"})
         (absent? {:fn/text "item3"})
         (exists? {:fn/text "item4"})
         (exists? {:fn/text "item5"}))
       (testing "item2 has category"
         (e/has-text? (h/completion-list-item "item2") "category1"))
       (testing "item1 does not have it (just to make sure selector is working"
         (e/has-text? (h/completion-list-item "item1") "category1"))
       (testing "items 2, 4 has empty hour display"
         (absent? (str (h/completion-list-item "item1") (h/descendant-with-class :timepicker-empty)))
         (exists? (str (h/completion-list-item "item2") (h/descendant-with-class :timepicker-empty)))
         (exists? (str (h/completion-list-item "item4") (h/descendant-with-class :timepicker-empty)))
         (absent? (str (h/completion-list-item "item5") (h/descendant-with-class :timepicker-empty))))))))

(deftest ^:parallel  completion-editing-deletion-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (testing "setup"
       (is (some? (f/new-user)))
       (f/goto-panel :nav-habits)
       (f/click (btn :add-new-habit))
       (f/add-category ["category1"])
       (testing "adding item to edit"
         (f/add-completion ["item-to-edit"]))
       (testing "adding item to delete"
         (f/add-completion ["item-to-delete"]))
       (f/click :habit-tab-completions))
     (exists? (h/completion-list-item "item-to-delete") "item-to-delete shows up")
     (exists? (h/completion-list-item "item-to-edit") "item-to-edit shows up")
     (testing "deleting"
       (f/click (str (h/completion-list-item "item-to-delete") (h/descendant-with-tag :completion-list-delete)))
       (absent? (h/completion-list-item "item-to-delete") "deleted")
       (exists? (h/completion-list-item "item-to-edit")) "other still there")
     (testing "editing"
       (f/click (str (h/completion-list-item "item-to-edit") (h/descendant-with-tag :completion-list-edit)))
       (f/fill (textarea :completion-edit-note) "item-after-edit")
       (f/click (any :completion-edit-type-dropdown))
       (f/click [(any :completion-edit-type-dropdown) {:fn/text "category1"}])
       (f/click (btn :simple-datepicker-pick))
       (f/click (btn :advanced-datepicker-confirm))
       (f/click :completion-edit-confirm)
       (testing "name category and timer changed"
         (e/has-text? (h/completion-list-item "item-after-edit") "category1")
         (exists? (str (h/completion-list-item "item-after-edit") (h/descendant-with-class :timepicker-empty)))))
     (testing "persistance"
       (e/refresh)
       (f/click :habit-tab-completions)
       (exists? (h/completion-list-item "item-after-edit")) ;; waiting until it loads
       (e/has-text? (h/completion-list-item "item-after-edit") "category1")
       (absent? (h/completion-list-item "item-to-delete"))))))
