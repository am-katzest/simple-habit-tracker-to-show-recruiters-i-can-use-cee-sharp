(ns frontend-test.fragments
  (:require [clojure.test :refer [deftest testing is are]]
            [frontend-test.setup :as s :refer-macros true]
            [frontend-test.helpers :refer [btn input wait-enabled wait-disabled wait-exists wait-predicate query] :as h  :refer-macros true]
            [etaoin.api :as e]))
(defn click [p]
  (s/use-driver
   e/go
   (wait-enabled p)
   (e/click p)))

(defn fill [p text]
  (s/use-driver
   e/go
   (wait-enabled p)
   (e/clear p)
   (e/fill p text)))

(defn goto-login []
  (s/use-driver
   e/go
   (e/go s/ROOT)
   (click (btn :login))
   (wait-exists (btn :login-button false))))

(defn goto-register []
  (s/use-driver
   e/go
   (e/go s/ROOT)
   (click (btn :new-account))
   (wait-exists (btn :register-button false))))

(defn login-as [username password]
  (s/use-driver
   e/go
   (goto-login)
   (fill (input :login-password) password)
   (wait-disabled (btn :login-button))
   (fill (input :login-username) username)
   (wait-enabled (btn :login-button))
   (e/wait 0.1)
   (click (btn :login-button))
   (wait-predicate #(or (e/exists? (btn :nav-logout))
                        (e/has-text? "invalid username or password"))) ; placeholder (only english)
   (e/exists? (btn :nav-logout))))

(defn create-user [username password]
  (s/use-driver
   e/go
   (goto-register)
   (fill (input :register-username) username)
   (fill (input :register-password) password)
   (fill (input :register-password2) password)
   (click (btn :register-button))))

(defn wait-logged-in []
  (s/use-driver
   e/go
   (wait-exists (btn :nav-logout))))

(defn new-user []
  (let [username (h/random-str)
        password (h/random-str)]
    (create-user username password)
    (wait-logged-in)
    username))

(defn goto-panel [nav]
  (s/use-driver
   e/go
   (e/click (btn nav))
   (wait-exists (assoc (btn nav) :fn/has-class :nav-disabled))))
