(ns frontend-test.login
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input wait-enabled wait-disabled wait-exists wait-predicate query random-str lazy-is exists? absent?] :as h  :refer-macros true]
            [frontend-test.fragments :as f]
            [etaoin.api :as e]))

(deftest ^:parallel login-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (e/go s/ROOT)
     (wait-exists {:tag :button})
     (testing "changes url"
       (lazy-is (= "?panel=login" (query (e/get-url)))))
     (testing "option screen is shown"
       (lazy-is (e/has-text? (btn :login) "login"))
       (is (e/has-text? (btn :new-account) "new account")))
     (testing "logging in negative wrong cred"
       (is (not (f/login-as (random-str) (random-str)))))
     (testing "user creation negative, checking works"
       (f/goto-register)
       (lazy-is (e/exists? (btn :register-button false)))
       (e/fill (input :register-username) "meow")
       (e/fill (input :register-password) "awawa")
       (e/fill (input :register-password2) "awawa")
       (wait-disabled (btn :register-button))
       (e/clear (input :register-password))
       (e/fill (input :register-password) "awawawawa")
       (wait-disabled (btn :register-button))
       (e/clear (input :register-password2))
       (e/fill (input :register-password2) "awawawawa")
       (wait-enabled (btn :register-button))
       (e/clear (input :register-username))
       (e/fill (input :register-username) "aw")
       (wait-disabled (btn :register-button))
       (is (e/exists? (btn :register-button false))))
     (let [username (random-str)
           password (random-str)]
       (testing "user creation positive"
         (f/create-user username password)
         (f/wait-logged-in)
         (testing "query changed"
           (is (= "?panel=habits" (query (e/get-url)))))
         (testing "username visible"
           (f/goto-panel :nav-account)
           (lazy-is (e/has-text? username))))
       (testing "url sets correctly when token exits"
         (e/go s/ROOT)
         (f/wait-logged-in)
         (is (= "?panel=habits" (query (e/get-url)))))
       (testing "logging out"
         (e/click (btn :nav-logout))
         (wait-exists (btn :login))
         (is (= "?panel=login" (query (e/get-url)))))
       (testing "logging in"
         (is (f/login-as username password))
         (testing "url changed"
           (lazy-is (= "?panel=habits" (query (e/get-url)))))
         (testing "username visible"
           (f/goto-panel :nav-account)
           (lazy-is (e/has-text? username))))))))

(deftest ^:parallel login-navigation-buttons-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (e/go s/ROOT)
     (testing "initial"
       (exists? (btn :login))
       (exists? (btn :new-account)))
     (testing "go-back button exists on register form"
       (f/click (btn :new-account))
       (exists? (btn :register-button))
       (exists? (btn :register-go-back-button)))
     (testing "go-back button on register form works"
       (f/click (btn :register-go-back-button))
       (exists? (btn :login))
       (exists? (btn :new-account))
       (absent? (btn :register-button)))
     (testing "go-back button exists on login form"
       (f/click (btn :login))
       (exists? (btn :login-button))
       (exists? (btn :login-go-back-button)))
     (testing "go-back button on login form works"
       (f/click (btn :login-go-back-button))
       (exists? (btn :login))
       (exists? (btn :new-account))
       (absent? (btn :login-button))))))

(comment
  (def d (e/firefox))
  (e/go d s/ROOT)
  (e/has-text? d (btn :login) "login")
  (e/exists? d (input :login-password))
  (e/fill d (input :login-password) "mwe")
  (e/fill))
