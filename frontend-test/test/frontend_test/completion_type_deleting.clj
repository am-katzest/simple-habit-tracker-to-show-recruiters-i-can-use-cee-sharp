(ns frontend-test.completion-type-deleting
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input exists? absent? lazy-is any textarea] :as h  :refer-macros true]
            [frontend-test.fragments :as f]
            [etaoin.api :as e]))

(deftest ^:parallel  completion-type-deleting
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (testing "setup"
       (is (some? (f/new-user)))
       (f/goto-panel :nav-habits)
       (f/click (btn :add-new-habit))
       (f/add-category ["cat1"])
       (f/add-category ["cat2"])
       (f/add-category ["cat3"])
       (f/add-category ["cat4"])
       (f/add-completion ["c1" nil "cat1"])
       (f/add-completion ["c2" nil "cat2"])
       (f/add-completion ["c3" nil "cat3"])
       (f/add-completion ["c4" nil "cat4"])
       ;; can't test colors because of react color picker
       (testing "not deleting cat1"
         (f/click (h/list-item "cat1"))
         (f/click :ct-edit-delete)
         (f/click :delete-popup-cancel))
       (testing "removing children of cat2"
         (f/click (h/list-item "cat2"))
         (f/click :ct-edit-delete)
         (f/click :delete-popup-delete-checkbox)
         (f/click :delete-popup-confirm))
       (testing "leaving note on cat3"
         (f/click (h/list-item "cat3"))
         (f/click :ct-edit-delete)
         (absent? :delete-popup-note)
         (f/click :delete-popup-note-checkbox)
         (f/fill   :delete-popup-note "meow")
         (f/click :delete-popup-note-checkbox)
         (f/click :delete-popup-note-checkbox)
         (f/click :delete-popup-confirm))
       (testing "leaving completions with cat4 unchanged"
         (f/click (h/list-item "cat4"))
         (f/click :ct-edit-delete)
         (f/click :delete-popup-confirm))
       (testing "looking at results"
         (e/refresh)
         (f/click :habit-tab-completions)
         (lazy-is (e/has-text? (h/completion-list-item "c1") "cat1") "c1 unchanged")
         (absent? (h/completion-list-item "c2") "c2 deleted")
         (lazy-is (e/has-text? (h/completion-list-item "c3") "meow") "c3 has added note")
         (exists? (h/completion-list-item "c4") "c4 exists"))))))
