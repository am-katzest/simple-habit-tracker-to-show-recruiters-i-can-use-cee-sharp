(ns frontend-test.completion-adding
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
       (f/click :habit-tab-cts)
       (f/click :add-new-ct)
       (f/fill :ct-edit-name "category1")
       (f/click :ct-edit-save))
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
         (f/click :add-new-completion-button)
         (f/click (btn :simple-datepicker-now))
         (f/fill (textarea :completion-edit-note) "item1")
         (f/click :completion-edit-confirm))
       (testing "adding today with category"
         (f/click :add-new-completion-button)
         (f/click (btn :simple-datepicker-now))
         (f/fill (textarea :completion-edit-note) "item2")
         (f/click (any :completion-edit-type-dropdown))
         (f/click [(any :completion-edit-type-dropdown) {:fn/text "category1"}])
         (f/click :completion-edit-confirm))
       (testing "adding yesterday"
         (f/click :add-new-completion-button)
         (f/click (btn :simple-datepicker-yesterday))
         (f/fill (textarea :completion-edit-note) "item3")
         (f/click :completion-edit-confirm))
       ;; i don't want to mess with dates too much (beyond today / not today)
       ;; because calendar seems like a pain to use programmatically
       ;; and having tests fail on 1st of the month for no particular reason sounds like a nightmare
       ;; now only midnight is problematic
       (testing "adding date pick no time"
         (f/click :add-new-completion-button)
         (f/click (btn :simple-datepicker-pick))
         (f/click (btn :advanced-datepicker-confirm))
         (f/fill (textarea :completion-edit-note) "item4")
         (f/click :completion-edit-confirm))
       (testing "adding date pick time"
         (f/click :add-new-completion-button)
         (f/click (btn :simple-datepicker-pick))
         (f/fill (any :advanced-datepicker-time-input) "11:11")
         (f/click (btn :advanced-datepicker-confirm))
         (f/fill (textarea :completion-edit-note) "item5")
         (f/click :completion-edit-confirm)))
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
       (testing "item 4 has empty hour display"
         (exists? (str (h/completion-list-item "item4") (h/descendant-with-class :timepicker-empty)))
         (absent? (str (h/completion-list-item "item1") (h/descendant-with-class :timepicker-empty)))
         (absent? (str (h/completion-list-item "item2") (h/descendant-with-class :timepicker-empty)))
         (absent? (str (h/completion-list-item "item5") (h/descendant-with-class :timepicker-empty))))))))
