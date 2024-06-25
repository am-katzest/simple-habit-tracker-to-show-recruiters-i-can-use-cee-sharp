(ns frontend.views
  (:require
   [re-frame.core :as re-frame]
   [re-com.core :as re-com :refer [at]]
   [reagent.core :as r]
   [frontend.localization :refer [tr]]
   [frontend.styles :as styles]
   [frontend.events :as e]
   [frontend.subs :as subs]))

(def <sub re-frame.core/subscribe)
(def >evt re-frame.core/dispatch)

(defn status [x] (if x :success :warning))

(defn register-form []
  (let [username (r/atom "")
        password (r/atom "")
        password2 (r/atom "")]
    (fn []
      (let [username-ok (> (count @username) 2)
            password-ok (> (count @password) 8)
            password2-ok (and (> (count @password2) 8) (= @password @password2))
            ready? (and username-ok password-ok password2-ok)
            submit (when ready? (fn [] (>evt [::e/create-new-account @username @password])))]
        [re-com/v-box
         :children
         [[re-com/label :label (tr :login/username)]
          [re-com/input-text
           :change-on-blur? false
           :model username
           :status (status username-ok)
           :on-change #(reset! username %)]
          [re-com/label :label (tr :login/password)]
          [re-com/input-password
           :change-on-blur? false
           :model password
           :status (status password-ok)
           :on-change #(reset! password %)]
          [re-com/label :label (tr :login/repeat)]
          [re-com/input-password
           :change-on-blur? false
           :model password2
           :status (status password2-ok)
           :on-change #(reset! password2 %)]
          [re-com/button
           :label (tr :login/create-new-account)
           :on-click submit
           :disabled? (not ready?)]]]))))

(defn login-form []
  (let [username (r/atom "")
        password (r/atom "")]
    (fn []
      (let [username-ok (> (count @username) 1)
            password-ok (> (count @password) 1)
            ready? (and username-ok password-ok)
            submit (when ready? (fn [] (>evt [::e/ask-for-token-login @username @password])))]
        [re-com/v-box
         :children
         [[re-com/label :label (tr :login/username)]
          [re-com/input-text
           :change-on-blur? false
           :model username
           :status (status username-ok)
           :on-change #(reset! username %)]
          [re-com/label :label (tr :login/password)]
          [re-com/input-password
           :change-on-blur? false
           :model password
           :status (status password-ok)
           :on-change #(reset! password %)]
          [re-com/button
           :label (tr :login/login)
           :on-click submit
           :disabled? (not ready?)]]]))))

(defn title []
  (let [name (<sub [::subs/name])]
    [re-com/title
     :src   (at)
     :label (str "Hello from :3" @name)
     :level :level1
     :class (styles/level1)]))

(defn login-register-panel []
  [:div.container-sm.md-2.center.bd-layout
   [:div.container
    [:div.row.justify-content-center
     [:div.col-md-4.pt-4
      (let [state (r/atom :select)]
        [(fn []
           (case @state
             :select
             [re-com/h-box
              :children [[re-com/button
                          :label (tr :login/register-new-account)
                          :class "btn-white"
                          :on-click #(reset! state :register)]
                         [re-com/button
                          :label (tr :login/login-old-account)
                          :class "btn-white"
                          :on-click #(reset! state :login)]]]
             :login
             [login-form]
             :register
             [register-form]))])]]]])

(defn navbar []
  [:nav.navbar.navbar-light.bg-light
   [:div.container-fluid
    [:a.navbar-brand "habit tracker"]
    [re-com/h-box
     :gap "20px"
     :children [[re-com/button
                 :label (tr :nav/account)
                 :class "btn-white"
                 :on-click #(>evt [::e/account-panel])]
                [re-com/button
                 :label (tr :nav/home)
                 :class "btn-white"
                 :on-click #(>evt [::e/home-panel])]
                [re-com/button
                 :label (tr :nav/logout)
                 :class "btn-white"
                 :on-click #(>evt [::e/logout])]]]]])

(defn home-panel []
  [re-com/title
   :src   (at)
   :label "meow (i'm home panel placeholder)"
   :level :level1
   :class (styles/level1)])

(defn account-panel []
  (let [user (<sub [::subs/user])]
    [re-com/title
     :src   (at)
     :label (str "meow, current user is" (:displayName user))
     :level :level1
     :class (styles/level1)]))

(def panels {:login login-register-panel
             :home home-panel
             :account account-panel})

(defn alerts []
  (let [alerts (<sub [::subs/alerts])]
    (when-not (empty? @alerts)
      [:div.position-fixed.bottom-0.end-0.p-3
       (re-com/v-box
        :gap "20px"
        :children (map
                   (fn [a] (apply re-com/alert-box :closeable? true :on-close #(>evt [::e/close-alert (:id a)]) (apply concat a))) @alerts))])))

(defn main-panel []
  (let [panel (or (panels @(<sub [::subs/panel])) title)]
    (if (= :login @(<sub [::subs/panel]))
      [:div [panel]
       [alerts]]
      [:div
       [navbar]
       [panel]
       [alerts]])))
