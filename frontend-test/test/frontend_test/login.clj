(ns frontend-test.login
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input wait-enabled wait-disabled wait-exists wait-predicate query] :as h  :refer-macros true]
            [etaoin.api :as e]))

(defn goto-login []
  (s/use-driver
   e/go
   (e/go s/ROOT)
   (wait-exists (btn :login))
   (e/click (btn :login))
   (wait-exists (btn :login-button))
   (is (e/exists? (btn :login-button false)))))

(defn goto-register []
  (s/use-driver
   e/go
   (e/go s/ROOT)
   (wait-exists (btn :new-account))
   (e/click (btn :new-account))
   (wait-exists (btn :register-button))
   (is (e/exists? (btn :register-button false)))))

(defn login-as [username password]
  (s/use-driver
   e/go
   (goto-login)
   (is (e/exists? (btn :login-button false)))
   (e/fill (input :login-password) password)
   (wait-disabled (btn :login-button))
   (e/fill (input :login-username) username)
   (wait-enabled (btn :login-button))
   (e/wait 0.1)
   (e/click (btn :login-button))
   (wait-predicate #(or (e/exists? (btn :nav-logout))
                        (e/has-text? "invalid username or password"))) ; placeholder (only english)
   (e/exists? (btn :nav-logout))))

(defn- create-user [username password]
  (s/use-driver
   e/go
   (goto-register)
   (e/fill (input :register-username) username)
   (e/fill (input :register-password) password)
   (e/fill (input :register-password2) password)
   (wait-enabled (btn :register-button))
   (e/click (btn :register-button))))

(defn random-str []
  (reduce str (take 15 (str (random-uuid)))))

(deftest login-test
  (s/use-new-driver
   e/go
   (h/with-wait h/short-wait
     (e/go s/ROOT)
     (wait-exists {:tag :button})
     (testing "changes url"
       (is (= "?panel=login" (query (e/get-url)))))
     (testing "option screen is shown"
       (is (e/has-text? (btn :login) "login"))
       (is (e/has-text? (btn :new-account) "new account")))
     (testing "logging in negative wrong cred"
       (is (not (login-as (random-str) (random-str)))))
     (testing "user creation negative, checking works"
       (goto-register)
       (is (e/exists? (btn :register-button false)))
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
         (create-user username password)
         (testing "username visible"
           (e/has-text? username))
         (wait-exists (btn :nav-logout))
         (testing "query changed"
           (is (= "?panel=habits" (query (e/get-url))))))
       (testing "url sets correctly when token exits"
         (e/go s/ROOT)
         (wait-exists (btn :nav-logout))
         (is (= "?panel=habits" (query (e/get-url)))))
       (testing "logging out"
         (e/click (btn :nav-logout))
         (wait-exists (btn :login))
         (is (= "?panel=login" (query (e/get-url)))))
       (testing "logging in"
         (is (login-as username password))
         (testing "url changed"
           (is (= "?panel=habits" (query (e/get-url)))))
         (testing "username visible"
           (e/has-text? username)))))))

(comment
  (def d (e/firefox))
  (e/go d s/ROOT)
  (e/has-text? d (btn :login) "login")
  (e/exists? d (input :login-password))
  (e/fill d (input :login-password) "mwe")
  (e/fill))
