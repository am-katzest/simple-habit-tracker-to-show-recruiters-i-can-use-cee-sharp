(ns frontend.views
  (:require
   [re-frame.core :as re-frame]
   [re-com.core :as re-com :refer [at]]
   [cljs-time.core :as time]
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

(defn join-keyword-ns ":a/b -> :a-b" [kw]
  (let [kw (keyword kw)]
    (if-let [ns (namespace kw)]
      (keyword (str ns "-" (name kw)))
      kw)))

(defn join-keywords [a b]
  (keyword
   (str (name (join-keyword-ns a))
        "-"
        (name (join-keyword-ns b)))))

(defn tag [tag & kvs]
  (apply assoc {} :data-testid (join-keyword-ns tag) kvs))

(defn make-tag [part1]
  (fn [part2 & rest]
    (apply tag (join-keywords part1 part2) rest)))

(defn confirm-panel [text confirm cancel attr]
  [re-com/modal-panel
   :attr attr
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
               :attr (tag :confirm-panel-confirm)
               :class "btn btn-primary"
               :label (tr :prompt/confirm)
               :on-click confirm]
              [re-com/button
               :attr (tag :confirm-panel-cancel)
               :class "btn btn-secondary"
               :label (tr :prompt/cancel)
               :on-click cancel]]]]]])

(defn save-undo-delete [base modified? valid? save undo delete deletion-confirm-text]
  (let [deleting? (r/atom false)]
    [(fn []
       (let [tag (make-tag base)]
         [re-com/h-box
          :gap "5px"
          :align :center
          :children
          [[re-com/md-icon-button
            :md-icon-name "zmdi-save"
            :attr (tag :save)
            :on-click save
            :disabled? (not (and modified? valid?))]
           (if modified?
             [re-com/md-icon-button
              :attr (tag :undo)
              :on-click undo
              :md-icon-name "zmdi-undo"]
             [re-com/md-icon-button
              :attr (tag :delete)
              :on-click #(reset! deleting? true)
              :md-icon-name "zmdi-delete"])
           (when @deleting?
             [confirm-panel
              deletion-confirm-text
              delete
              #(reset! deleting? false)
              (tag :confirm-delete)])]]))]))

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

(defn single-habit-info-edit-panel [id]
  (let [original @(<sub [::subs/habit id])
        state (r/atom original)]
    [(fn [] (let [normalized (dh/normalize-habit @state)
                  modified? (not= original normalized)
                  valid? (dh/validate-habit normalized)]
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
                            [save-undo-delete :habit-edit modified? valid?
                             #(>evt [::e/update-habit (dh/normalize-habit @state)])
                             #(reset! state original)
                             #(>evt [::e/delete-habit (:id original)])
                             (tr :habit/confirm-deletion)]]]
                [re-com/input-textarea
                 :attr (tag :habit-edit-description)
                 :width "280px"
                 :change-on-blur? false
                 :placeholder (tr :habit/description)
                 :model (-> @state :description str) ; can be null
                 :on-change #(swap! state assoc :description %)]]]))]))

(defn tabs [selected tabs f]
  ;; re-com says it provides this, but it's borked
  (into [:ul.nav.nav-tabs]
        (for [[id label data-tag] tabs]
          [:li.nav-item
           (if (= id selected)
             [:a.nav-link.active (tag data-tag :on-click #(f nil)) label]
             [:a.nav-link (tag data-tag :on-click #(f id)) label])])))

(defn habit-subpanel-control [id selected-subpanel]
  [re-com/h-box
   :align :end
   :children
   [[re-com/v-box
     :children
     [[re-com/box
       :align :center
       :child [re-com/label :label (tr :habit/tabs-select)]]
      [tabs @selected-subpanel
       [[:cts (tr :habit/tab-cts) :habit-tab-cts]
        [:alerts (tr :habit/tab-alerts) :habit-tab-alerts]
        [:completions (tr :habit/tab-completions) :habit-tab-completions]]
       #(reset! selected-subpanel %)]]]]])

(defn completion-type-label [{:keys [name color]}]
  [re-com/h-box
   :justify :between
   :children [[:label {:style {:overflow-x :hidden :width "145px"}} name]
              [:div {:style {:background-color color
                             :border-radius "10px"
                             :width "25px"
                             :height "25px"}}]]])

(defn completion-type-list []
  (let [habit-id @(<sub [::subs/selected-habit])
        cts @(<sub [::subs/selected-habit-cts])
        selected @(<sub [::subs/selected-ct])]
    [re-com/box
     :width "200px"
     :child
     (into [:div.list-group.w-100
            [:button.list-group-item.list-group-item-action.list-group-item-dark
             (tag :add-new-ct :on-click #(>evt [::e/new-empty-ct habit-id]))
             (tr :ct/add-new)]]
           (mapv (fn [[id ct]]
                   [:button.list-group-item.list-group-item-action.w-100
                    (let [select #(>evt [::e/select-ct habit-id id])]
                      (if (= id selected)
                        (tag :ct-list-item-selected :class :active :on-click select)
                        (tag :ct-list-item :on-click select)))
                    [completion-type-label ct]])
                 cts))]))

(defn completion-type-edit-panel [ctid hid]
  (let [original @(<sub [::subs/selected-ct-data])
        state (r/atom original)]
    [(fn []
       (let [normalized (dh/normalize-ct @state)
             modified? (not= original normalized)
             valid? (dh/validate-ct normalized)]
         [re-com/v-box
          :margin "20px"
          :gap "20px"
          :children
          [[re-com/input-text
            :attr (tag :ct-edit-name)
            :status (status-no-green valid?)
            :model (:name  @state)
            :change-on-blur? false
            :on-change #(swap! state assoc :name %)]
           [re-com/input-textarea
            :attr (tag :ct-edit-description)
            :change-on-blur? false
            :model (:description  @state)
            :on-change #(swap! state assoc :description %)]
           [re-com/h-box
            :justify :between
            :children [[:input (tag :ct-edit-color
                                    :type :color
                                    :value (:color @state)
                                    :on-change #(swap! state assoc :color (.-value (.-target %))))]
                       [save-undo-delete :ct-edit modified? valid?
                        #(>evt [::e/update-ct hid ctid (dh/normalize-ct @state)])
                        #(reset! state original)
                        #(>evt [::e/delete-ct hid ctid])
                        (tr :ct/confirm-deletion)]]]]]))]))

(defn ct-subpanel []
  [re-com/h-box
   :margin "20px"
   :children [[completion-type-list]
              (when-let [id @(<sub [::subs/selected-ct])]
                [completion-type-edit-panel id @(<sub [::subs/selected-habit])])]])

(defn advanced-datepicker [confirm cancel]
  (let [t (dh/time-now)
        hours-minutes (r/atom (dh/date-time->hours-minutes t))
        date (r/atom t)
        exact-time? (r/atom false)]
    [(fn [] [re-com/h-box
             :children
             [[re-com/datepicker
               :model date
               :attr (tag :advanced-datepicker-datepicker)
               :on-change #(reset! date %)]
              [re-com/v-box
               :margin "20px"
               :align :center
               :children
               [[re-com/label :label (tr :completion/specify-hour)]
                [re-com/gap :size "10px"]
                [re-com/h-box
                 :width "90px"
                 :align :center
                 :gap "20px"
                 :children
                 [[re-com/checkbox
                   :attr (tag :advanced-datepicker-time-checkbox)
                   :model @exact-time?
                   :on-change #(reset! exact-time? %)]
                  [re-com/input-time
                   :attr (tag :advanced-datepicker-time-input)
                   :model @hours-minutes
                   :on-change (fn [x]
                                (reset! exact-time? true)
                                (reset! hours-minutes x))]]]
                [re-com/gap :size "30px"]
                [re-com/h-box
                 :width "150px"
                 :justify :between
                 :align :center
                 :children
                 [[re-com/button :class "btn btn-secondary"
                   :attr (tag :advanced-datepicker-cancel)
                   :label (tr :completion/datepicker-cancel)
                   :on-click cancel]
                  [re-com/button :class "btn btn-primary"
                   :attr (tag :advanced-datepicker-confirm)
                   :label (tr :completion/datepicker-confirm)
                   :on-click #(confirm [@exact-time? (dh/set-hours-minutes @date @hours-minutes)])]]]]]]])]))

(defn simple-date-picker [model]
  (let [current-state (r/atom :invalid)
        type (fn [tag] (if (= tag @current-state)
                         :button.btn.btn-secondary
                         :button.btn.btn-light))
        switch (fn [tag date precision]
                 (fn []
                   (reset! current-state tag)
                   (reset! model [precision (date)])))]
    [(fn []
       [:<>
        [(cond
           (or (vector? @current-state) (= :invalid @current-state))
           :div.input-group.form-control.is-invalid.btn-group
           :else :div.input-group.form-control.is-valid.btn-group)
         [(type :now)
          (tag :simple-datepicker-now
               :type :button
               :on-click (switch :now dh/time-now true))
          (tr :completion/date-now)]
         [(type :today)
          (tag :simple-datepicker-today
               :type :button
               :on-click (switch :today dh/time-now false))
          (tr :completion/date-today)]
         [(type :yesterday)
          (tag :simple-datepicker-yesterday
               :type :button
               :on-click (switch :yesterday #(time/minus (dh/time-now) (time/days 1)) false))
          (tr :completion/date-yesterday)]
                                        ; vector means advanced picker is active
         [(type :pick)
          (tag :simple-datepicker-pick
               :type :button
               :on-click #(swap! current-state (fn [old] [old])))
          (tr :completion/date-pick)]]
        (when (vector? @current-state)
          [:div.dropdown-menu.show
           [advanced-datepicker
            (fn [date]
              (reset! model date)
              (reset! current-state :pick))
            #(swap! current-state first)]])])]))

(defn color-editor [use-color? color]
  [re-com/h-box
   :gap "20px"
   :align :center
   :children
   [[re-com/checkbox
     :attr (tag :colorpicker-toggle)
     :model use-color?
     :on-change #(reset! use-color? %)]
    [re-com/label :label (tr :completion/use-color)]
    [:input (tag :colorpicker
                 :type :color
                 :value @color
                 :on-change (fn [val]
                              (reset! use-color? true)
                              (reset! color (.-value (.-target val)))))]]])

(defn completion-type-selection-dropbox [id]
  [re-com/single-dropdown
   :attr (tag :completion-edit-type-dropdown)
   :model id
   :width "100%"
   :choices (into [{}] (map second @(<sub [::subs/selected-habit-cts])))
   :label-fn #(when (:id %) (:name %))
   :render-fn #(if (:id %) (completion-type-label %) [re-com/label :label "none" :style {:color "#666"}])
   :on-change #(reset! id %)])

(defn completion-edit [id initial cancel accept]
  (let [date (r/atom [(:isExactTime initial) (:completionDate initial)])
        note (r/atom (:note initial))
        use-color? (r/atom (some? (:color initial)))
        color (r/atom (or (:color initial) "#ffffff"))
        ct-id (r/atom (:completionTypeId initial))]
    [(fn [] [re-com/modal-panel
             :backdrop-on-click cancel
             :child
             [re-com/v-box
              :min-width "300px"
              :children
              [[re-com/label :label (tr :completion/new-completion)]
               [re-com/gap :size "20px"]
               [simple-date-picker date]
               [re-com/gap :size "20px"]
               [re-com/label :label (tr :completion/note)]
               [re-com/input-textarea
                :attr (tag :completion-edit-note)
                :model note
                :width "100%"
                :on-change #(reset! note %)]
               [re-com/gap :size "20px"]
               [color-editor use-color? color]
               [re-com/gap :size "20px"]
               [re-com/label :label (tr :completion/type)]
               [completion-type-selection-dropbox ct-id]
               [re-com/gap :size "20px"]
               [re-com/h-box
                :justify :between
                :children
                [[re-com/button
                  :attr (tag :completion-edit-confirm)
                  :class (str "btn btn-primary" (when-not @date " disabled"))
                  :on-click #(when @date (accept (dh/normalize-completion
                                                  {:completionTypeId @ct-id
                                                   :color (when @use-color? @color)
                                                   :completionDate (second @date)
                                                   :isExactTime (first @date)
                                                   :note @note})))
                  :label (tr :completion/add-new-confirm)]
                 [re-com/button
                  :attr (tag :completion-edit-cancel)
                  :on-click cancel
                  :class "btn btn-secondary"
                  :label (tr :completion/cancel)]]]]]])]))

(defn habit-subpanel-add-completion [id]
  (let [adding? (r/atom false)]
    [(fn []
       [:<>
        [re-com/box
         :align :center
         :align-self :center
         :margin "30px"
         :child [re-com/button
                 :class "btn btn-primary"
                 :label (tr :habit/add-new)
                 :attr (tag :add-new-completion-button)
                 :on-click #(reset! adding? true)]]
        (when @adding?
          [completion-edit {} id
           #(reset! adding? false)
           (fn [x]
             (reset! adding? false)
             (>evt [::e/add-completion id x]))])])]))

(defn habits-panel-right-part [id]
  (let [selected-subpanel (r/atom nil)]
    [(fn []
       (re-com/v-box
        :class (styles/habit-panel-right)
        :children [(re-com/h-box
                    :children [[single-habit-info-edit-panel id]
                               [re-com/v-box
                                :justify :between
                                :children
                                [[habit-subpanel-add-completion id]
                                 [habit-subpanel-control id selected-subpanel]]]])
                   [:div {:class (styles/habit-subpanel)}
                    (case @selected-subpanel
                      :cts [ct-subpanel]
                      :alerts nil
                      :completions "meow"
                      nil nil)]]))]))

(defn habits-panel []
  [re-com/h-box
   :children [[habit-list]
              (when-let [id @(<sub [::subs/selected-habit])]
                [habits-panel-right-part id])]])

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
