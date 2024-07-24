(ns frontend-test.fragments
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

(defn create-user [username password]
  (s/use-driver
   e/go
   (goto-register)
   (e/fill (input :register-username) username)
   (e/fill (input :register-password) password)
   (e/fill (input :register-password2) password)
   (wait-enabled (btn :register-button))
   (e/click (btn :register-button))))
