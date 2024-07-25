(ns frontend.views
  (:require
   [re-frame.core :as re-frame]
   [re-com.core :as re-com :refer [at]]
   [reagent.core :as r]
   [frontend.localization :refer [tr]]
   [frontend.styles :as styles]
   [frontend.events :as e]
   [frontend.data-helpers :as dh]
   [frontend.subs :as subs]))

(def <sub re-frame.core/subscribe)
(def >evt re-frame.core/dispatch)

(defn status [x] (if x :success :warning))

(defn status-no-green [x] (if x nil :warning))

(defn tag [tag & kvs]
  (apply assoc {} :data-testid tag kvs))

(defn confirm-panel [text confirm cancel]
  [re-com/modal-panel
   :backdrop-on-click cancel
   :child [re-com/v-box
           :gap "30px"
           :children
           [text
            [re-com/h-box
             :align :center
             :gap "30px"
             :children
             [[re-com/button
               :class "btn btn-primary"
               :label (tr :prompt/confirm)
               :on-click confirm]
              [re-com/button
               :class "btn btn-secondary"
               :label (tr :prompt/cancel)
               :on-click cancel]]]]]])

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
           :attr (tag :register-username :name :username)
           :change-on-blur? false
           :model username
           :status (status username-ok)
           :on-change #(reset! username %)]
          [re-com/label :label (tr :login/password)]
          [re-com/input-password
           :attr (tag :register-password :name :password)
           :change-on-blur? false
           :model password
           :status (status password-ok)
           :on-change #(reset! password %)]
          [re-com/label :label (tr :login/repeat)]
          [re-com/input-password
           :attr (tag :register-password2 :name :password2)
           :change-on-blur? false
           :model password2
           :status (status password2-ok)
           :on-change #(reset! password2 %)]
          [re-com/button
           :attr (tag :register-button :name :register-button)
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
           :attr (tag :login-username :name :username)
           :change-on-blur? false
           :model username
           :status (status username-ok)
           :on-change #(reset! username %)]
          [re-com/label :label (tr :login/password)]
          [re-com/input-password
           :attr (tag :login-password :name :password)
           :change-on-blur? false
           :model password
           :status (status password-ok)
           :on-change #(reset! password %)]
          [re-com/button
           :attr (tag :login-button :name :login-button)
           :label (tr :login/login)
           :on-click submit
           :disabled? (not ready?)]]]))))

(defn error-panel []
  [re-com/title
   :src   (at)
   :label "wrong panel, shouldn't happen"
   :level :level1
   :class (styles/level1)])

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
                          :attr (tag :new-account)
                          :label (tr :login/register-new-account)
                          :class "btn-white"
                          :on-click #(reset! state :register)]
                         [re-com/button
                          :attr (tag :login)
                          :label (tr :login/login-old-account)
                          :class "btn-white"
                          :on-click #(reset! state :login)]]]
             :login
             [login-form]
             :register
             [register-form]))])]]]])

(defn navbar [panel]
  [:nav.navbar.navbar-light.bg-light
   [:div.container-fluid
    [:a.navbar-brand "habit tracker"]
    [re-com/h-box
     :gap "20px"
     :children (->> [[:account :nav-account :nav/account ::e/account-panel]
                     [:habits :nav-habits :nav/habits ::e/habits-panel]
                     [nil :nav-logout :nav/logout ::e/logout]]
                    (map (fn [[id nav trans evt]]
                           [re-com/button
                            :label (tr trans)
                            :class (if (= id panel) "btn-white nav-disabled" "btn-white")
                            :attr (tag nav)
                            :on-click #(>evt [evt])])))]]])


(defn habit-list []
  (let [habits @(<sub [::subs/habit-names])
        current @(<sub [::subs/selected-habit])]
    [re-com/box
     :width "200px"
     :child
     (into [:div.list-group.w-100
            [:button.list-group-item.list-group-item-action.list-group-item-dark
             (tag :add-new-habit :on-click #(>evt [::e/new-empty-habit]))
             (tr :habit/add-new)]]
           (mapv (fn [[id name]]
                   [:button.list-group-item.list-group-item-action.w-100
                    (let [select #(>evt [::e/select-habit id])]
                      (if (= id current)
                        (tag :habit-list-item-selected :class :active :on-click select)
                        (tag :habit-list-item :on-click select)))
                    name])
                 habits))]))

(defn single-habit-info-edit-panel [original state deleting?]
  (let [modified? (not= original (dh/normalize-habit @state))
        valid? (not (contains? #{nil ""} (:name @state)))]
    [re-com/v-box
     :margin "20px"
     :width "280px"
     :gap "20px"
     :children
     [[re-com/h-box
       :justify :between
       :children [[re-com/input-text
                   :attr (tag :habit-edit-name)
                   :width "200px"
                   :placeholder (tr :habit/name)
                   :status (status-no-green valid?)
                   :change-on-blur? false
                   :model (:name @state)
                   :on-change #(swap! state assoc :name %)]
                  [re-com/h-box
                   :gap "5px"
                   :align :center
                   :children
                   [[re-com/md-icon-button
                     :md-icon-name "zmdi-save"
                     :attr (tag :habit-edit-save)
                     :on-click #(>evt [::e/update-habit (dh/normalize-habit @state)])
                     :disabled? (not (and modified? valid?))]
                    (if modified?
                      [re-com/md-icon-button
                       :attr (tag :habit-edit-undo)
                       :on-click #(reset! state original)
                       :md-icon-name "zmdi-undo"]
                      [re-com/md-icon-button
                       :attr (tag :habit-edit-delete)
                       :on-click #(reset! deleting? true)
                       :md-icon-name "zmdi-delete"])]]]]
      [re-com/input-textarea
       :attr (tag :habit-edit-description)
       :width "280px"
       :change-on-blur? false
       :placeholder (tr :habit/description)
       :model (-> @state :description str) ; can be null
       :on-change #(swap! state assoc :description %)]
      (when @deleting?
        [confirm-panel
         (tr :habit/confirm-deletion)
         #(>evt [::e/delete-habit (:id original)])
         #(reset! deleting? false)])]]))

(defn single-habit-info-edit-panel-wrap [id]
  (let [original @(<sub [::subs/habit id])
        state (r/atom original)]
    [single-habit-info-edit-panel original state (r/atom false)]))

(defn habits-panel []
  [re-com/h-box
   :children [[habit-list]
              (when-let [id @(<sub [::subs/selected-habit])]
                [single-habit-info-edit-panel-wrap id])]])

(defn account-panel []
  (let [user @(<sub [::subs/user])]
    [re-com/title
     :src   (at)
     :label (str "meow, current user is" (:displayName user))
     :level :level1
     :class (styles/level1)]))


(def panels {:login login-register-panel
             :habits habits-panel
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
  (let [current-panel @(<sub [::subs/panel])]
    [:div
     (when-not (= :login current-panel) [navbar current-panel])
     [(or (panels current-panel) error-panel)]
     [alerts]]))
