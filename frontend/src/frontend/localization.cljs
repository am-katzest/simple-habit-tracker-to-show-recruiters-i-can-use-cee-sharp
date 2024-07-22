(ns frontend.localization
  (:require [re-frame.core :as re-frame]))

(defn tr [kw]
  (or (get @(re-frame/subscribe [:locale/map]) kw) (str "translate(:" kw ")")))

; there's definitely room for improvement
(def one-big-dictionary
  {:eng
   (merge #:login
           {:create-new-account "create new account"
            :login "login"
            :password "password"
            :repeat "repeat password"
            :username "username"
            :register-new-account "new account"
            :login-old-account "login"}
          #:nav
           {:logout "logout"
            :account "account"
            :habits "habits"}
           #:habit
           {:add-new "add new"}
          #:error
           {:wrong-cred "invalid username or password"})})
