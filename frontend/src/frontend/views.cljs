(ns frontend.views
  (:require
   [re-frame.core :as re-frame]
   [re-com.core :as re-com :refer [at]]
   [reagent.core :as r]
   [frontend.localization :refer [tr]]
   [frontend.styles :as styles]
   [frontend.subs :as subs]))

(def <sub re-frame.core/subscribe)
(def >evt re-frame.core/dispatch)

(defn status [x] (if x :success :warning))

(defn login-register-form []
  (let [username (r/atom "")
        password (r/atom "")
        password2 (r/atom "")
        new-account (r/atom false)]
    (fn []
      [re-com/v-box
       :children
       [[re-com/title :label
         (if @new-account (tr :login/create-new-account) (tr :login/login))]
        [re-com/v-box
         :children
         [[re-com/label :label (tr :login/username)]
          [re-com/input-text
           :change-on-blur? false
           :model username
           :status (status (> (count @username) 2))
           :on-change #(reset! username %)]
          [re-com/label :label (tr :login/password)]
          [re-com/input-password
           :change-on-blur? false
           :model password
           :status (status (> (count @password) 8))
           :on-change #(reset! password %)]
          [re-com/label :label (tr :login/repeat)]
          [re-com/input-password
           :change-on-blur? false
           :model password2
           :status (status (and (> (count @password2) 8) (= @password @password2)))
           :on-change #(reset! password2 %)]]]]])))

(defn title []
  (let [name (<sub [::subs/name])]
    [re-com/title
     :src   (at)
     :label (str "Hello from :3" @name)
     :level :level1
     :class (styles/level1)]))

(defn main-panel []
  [re-com/v-box
   :src      (at)
   :height   "100%"
   :children [[login-register-form]]])
