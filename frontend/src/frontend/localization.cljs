(ns frontend.localization
  (:require [re-frame.core :as re-frame]))

(defn tr [kw]
  (or (get @(re-frame/subscribe [:locale/map]) kw) (str "translate(:" kw ")")))

; there's definitely room for improvement
(defn tr-error [str]
  (get-in @(re-frame/subscribe [:locale/map]) [:error str]))
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
           {:add-new "add new"
            :description "add description.."
            :confirm-deletion "really delete this habit?"
            :tabs-select "select view"
            :tab-cts "categories"
            :tab-alerts "alerts"
            :tab-completions "completions"
            :new-habit "new habit"
            :name "add name.."}
           #:ct
           {:add-new "add new"
            :confirm-deletion "really delete this category?"
            :new-ct "category"}
           #:prompt{:confirm "confirm"
                    :cancel "cancel"}
          {:error {"duplicate username" {:part1 "username" :part2 "taken, pick another"}
                   "invalid username or password" "wrong username or password"
                   "invalid token" "token expired, please reload"
                   "habit not found" "habit not found, deleted?"
                   "completion type not found" "category not found, deleted?"}})})
