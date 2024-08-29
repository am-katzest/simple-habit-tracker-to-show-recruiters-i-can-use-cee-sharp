(ns frontend.views
  (:require
   [re-frame.core :as re-frame]
   [re-com.core :as re-com :refer [at]]
   [cljs-time.core :as time]
   [reagent.core :as r]
   [frontend.localization :refer [tr add-prefix]]
   [frontend.styles :as styles]
   [frontend.svg :as svg]
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

(defn add-prefix-tag [part1]
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
       (let [tag (add-prefix-tag base)]
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
              :on-click (if (nil? deletion-confirm-text)
                          delete
                          #(reset! deleting? true))
              :md-icon-name "zmdi-delete"])
           (when @deleting?
             [confirm-panel
              deletion-confirm-text
              delete
              #(reset! deleting? false)
              (tag :confirm-delete)])]]))]))

(defn register-form [go-back]
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
         :width "280px"
         :children
         [[re-com/label :label (tr :login/username)]
          [re-com/input-text
           :width "280px"
           :attr (tag :register-username :name :username)
           :change-on-blur? false
           :model username
           :status (status username-ok)
           :on-change #(reset! username %)]
          [re-com/gap :size "10px"]
          [re-com/label :label (tr :login/password)]
          [re-com/input-password
           :width "280px"
           :attr (tag :register-password :name :password)
           :change-on-blur? false
           :model password
           :status (status password-ok)
           :on-change #(reset! password %)]
          [re-com/gap :size "10px"]
          [re-com/label :label (tr :login/repeat)]
          [re-com/input-password
           :width "280px"
           :attr (tag :register-password2 :name :password2)
           :change-on-blur? false
           :model password2
           :status (status password2-ok)
           :on-change #(reset! password2 %)]
          [re-com/gap :size "10px"]
          [re-com/h-box
           :justify :between
           :children
           [[re-com/button
             :attr (tag :register-button :name :register-button)
             :label (tr :login/create-new-account)
             :class "btn btn-primary"
             :on-click submit
             :disabled? (not ready?)]
            [re-com/button
             :attr (tag :register-go-back-button)
             :label (tr :login.select/go-back)
             :on-click go-back
             :class "btn btn-secondary"]]]]]))))

(defn login-form [go-back]
  (let [username (r/atom "")
        password (r/atom "")]
    (fn []
      (let [username-ok (> (count @username) 1)
            password-ok (> (count @password) 1)
            ready? (and username-ok password-ok)
            submit (when ready? (fn [] (>evt [::e/ask-for-token-login @username @password])))]
        [re-com/v-box
         :width "280px"
         :children
         [[re-com/label :label (tr :login/username)]
          [re-com/input-text
           :width "280px"
           :attr (tag :login-username :name :username)
           :change-on-blur? false
           :model username
           :status (status username-ok)
           :on-change #(reset! username %)]
          [re-com/gap :size "10px"]
          [re-com/label :label (tr :login/password)]
          [re-com/input-password
           :width "280px"
           :attr (tag :login-password :name :password)
           :change-on-blur? false
           :model password
           :status (status password-ok)
           :on-change #(reset! password %)]
          [re-com/gap :size  "10px"]
          [re-com/h-box
           :justify :between
           :children
           [[re-com/button
             :attr (tag :login-button :name :login-button)
             :label (tr :login/login)
             :on-click submit
             :class "btn btn-primary"
             :disabled? (not ready?)]
            [re-com/button
             :attr (tag :login-go-back-button)
             :label (tr :login.select/go-back)
             :on-click go-back
             :class "btn btn-secondary"]]]]]))))

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
      (let [state (r/atom :select)
            reset (fn [x] (fn [] (reset! state x)))]
        [(fn []
           (case @state
             :select
             [re-com/h-box
              :children [[re-com/button
                          :attr (tag :new-account)
                          :label (tr :login.select/register-new-account)
                          :class "btn-white"
                          :on-click (reset :register)]
                         [re-com/button
                          :attr (tag :login)
                          :label (tr :login.select/login-old-account)
                          :class "btn-white"
                          :on-click (reset :login)]]]
             :login
             [login-form (reset :select)]
             :register
             [register-form (reset :select)]))])]]]])

(defn navbar [panel]
  (let [tr-nav (add-prefix :nav)
        tag-nav (add-prefix-tag :nav)]
    [:nav.navbar.navbar-light.bg-light
     [:div.container-fluid
      [:a.navbar-brand "habit tracker"]
      [re-com/h-box
       :gap "20px"
       :children (->> [[:account ::e/account-panel]
                       [:habits ::e/habits-panel]
                       [:logout ::e/logout]]
                      (map (fn [[id evt]]
                             [re-com/button
                              :label (tr-nav id)
                              :class (if (= id panel) "btn-white nav-disabled" "btn-white")
                              :attr (tag-nav id)
                              :on-click #(>evt [evt])])))]]]))

(defn habit-list []
  (let [habits @(<sub [::subs/habit-names])
        current @(<sub [::subs/selected-habit])]
    [re-com/box
     :width "200px"
     :child
     (into [:div.list-group.w-100
            [:button.list-group-item.list-group-item-action.list-group-item-dark
             (tag :add-new-habit :on-click #(>evt [::e/new-empty-habit (tr :habit/new-habit)]))
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
      (let [tr-tab (add-prefix [:habit :tab])]
        [tabs @selected-subpanel
         [[:cts (tr-tab :cts) :habit-tab-cts]
          [:alerts (tr-tab :alerts) :habit-tab-alerts]
          [:completions (tr-tab :completions) :habit-tab-completions]]
         #(reset! selected-subpanel %)])]]]])

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
             (tag :add-new-ct :on-click #(>evt [::e/new-empty-ct habit-id (tr :ct/add-new)]))
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
                        nil]]]]]))]))

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
        exact-time? (r/atom false)
        tr (add-prefix [:completion :editor :advanced-datepicker])]
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
               [[re-com/label :label (tr :specify-hour)]
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
                   :label (tr :cancel)
                   :on-click cancel]
                  [re-com/button :class "btn btn-primary"
                   :attr (tag :advanced-datepicker-confirm)
                   :label (tr :confirm)
                   :on-click #(confirm [@exact-time? (dh/set-hours-minutes @date @hours-minutes)])]]]]]]])]))

(defn simple-date-picker-internal [model initial options]
  (let [current-state (r/atom initial)
        type (fn [id] (if (= id @current-state)
                        :button.btn.btn-secondary
                        :button.btn.btn-light))
        switch (fn [id date precision]
                 (fn []
                   (reset! current-state id)
                   (reset! model [precision (date)])))]
    [(fn []
       [:<>
        [(cond
           (or (vector? @current-state) (= :invalid @current-state))
           :div.input-group.form-control.is-invalid.btn-group
           :else :div.input-group.form-control.is-valid.btn-group)
         (map (fn [[id test-id description other]]
                (let [f (if (coll? other)
                          (switch id (first other) (second other)) ; collection -> two arguments to switch,
                          other)]       ; function -> standalone
                  ^{:key id}
                  [(type id)
                   (tag test-id
                        :type :button
                        :on-click f)
                   description]))
              (concat options [[:pick :simple-datepicker-pick (tr :completion.date/pick) #(swap! current-state (fn [old] [old]))]]))]
        (when (vector? @current-state)
          [:div.dropdown-menu.show
           [advanced-datepicker
            (fn [date]
              (reset! model date)
              (reset! current-state :pick))
            #(swap! current-state first)]])])]))

(defn simple-date-picker [model]
  [simple-date-picker-internal
   model
   :invalid
   (let [tr-date (add-prefix [:completion :date])]
     [[0 :simple-datepicker-now (tr-date :now) [dh/time-now true]]
      [1 :simple-datepicker-today (tr-date :today) [dh/time-now false]]
      [2 :simple-datepicker-yesterday (tr-date :yesterday) [#(time/minus (dh/time-now) (time/days 1)) false]]])])

(defn simple-date-picker-already-present-variant [initial model]
  [simple-date-picker-internal
   model
   :existing
   [[:existing :simple-datepicker-unchanged (tr :completion.date/unchanged) [#(:completionDate initial) (:isExactTime initial)]]]])

(defn color-editor [use-color? color]
  [re-com/h-box
   :gap "20px"
   :align :center
   :children
   [[re-com/checkbox
     :attr (tag :colorpicker-toggle)
     :model use-color?
     :on-change #(reset! use-color? %)]
    [re-com/label :label (tr :completion.editor/use-color)]
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

(defn completion-edit [initial cancel accept title-text confirm-button-text]
  (let [initial-date [(:isExactTime initial) (:completionDate initial)]
        date (r/atom initial-date)
        note (r/atom (:note initial))
        use-color? (r/atom (some? (:color initial)))
        color (r/atom (or (:color initial) "#ffffff"))
        ct-id (r/atom (:completionTypeId initial))
        tr (add-prefix [:completion :editor])]
    [(fn [] [re-com/modal-panel
             :backdrop-on-click cancel
             :child
             [re-com/v-box
              :min-width "300px"
              :children
              [[re-com/label :label title-text]
               [re-com/gap :size "20px"]
               (if initial
                 [simple-date-picker-already-present-variant initial-date date]
                 [simple-date-picker date])
               [re-com/gap :size "20px"]
               [re-com/label :label (tr :note)]
               [re-com/input-textarea
                :attr (tag :completion-edit-note)
                :model note
                :width "100%"
                :on-change #(reset! note %)]
               [re-com/gap :size "20px"]
               [color-editor use-color? color]
               [re-com/gap :size "20px"]
               [re-com/label :label (tr :type)]
               [completion-type-selection-dropbox ct-id]
               [re-com/gap :size "20px"]
               [re-com/h-box
                :justify :between
                :children
                [[re-com/button
                  :attr (tag :completion-edit-confirm)
                  :class (str "btn btn-primary" (when-not (second @date) " disabled"))
                  :on-click #(when @date (accept (dh/normalize-completion
                                                  {:completionTypeId @ct-id
                                                   :color (when @use-color? @color)
                                                   :completionDate (second @date)
                                                   :isExactTime (first @date)
                                                   :note @note})))
                  :label confirm-button-text]
                 [re-com/button
                  :attr (tag :completion-edit-cancel)
                  :on-click cancel
                  :class "btn btn-secondary"
                  :label (tr :cancel)]]]]]])]))

(defn completion-add [cancel habit-id]
  [completion-edit nil
   cancel
   (fn [x]
     (cancel)
     (>evt [::e/add-completion habit-id x]))
   (tr :completion/new-completion)
   (tr :completion/add-new-confirm)])

(defn completion-change [cancel initial-id habit-id]
  (let [initial @(<sub [::subs/completion initial-id])]
    [completion-edit initial
     cancel
     (fn [x]
       (cancel)
       (>evt [::e/edit-completion habit-id (:id initial) x]))
     (tr :completion/edit-completion)
     (tr :completion/edit-confirm)]))

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
          [completion-add #(reset! adding? false) id])])]))

(defn calendar-cell [label date]
  (let [day-ids @(<sub [::subs/selected-habit-completion-ids-on-day date])
        colors (mapv (fn [id] (:color @(<sub [::subs/completion-with-fixed-color id]))) day-ids)]
    [:div {:style {:display :flex :width "100%" :height "100%"}}
     [:div {:style {:width "100%" :height "100%"}}
      [svg/make-calendar-outline colors]]
     [:div.position-relative {:style {:width "100%" :margin-left "-100%" :height "100%"}}
      [:span.position-absolute.top-50.start-50.translate-middle   label]]]))

(defn- history-datepicker-cell [habit-id model {:keys [label date disabled? class style attr selected focus-month]}]
  (>evt [::e/ensure-completion-history-month-is-downloaded habit-id date])
  (let [current-month? (= focus-month (time/month date))
        current-day? (time/equal? date selected)
        color (if current-month?  :black "#ccc")]
    [:td
     (-> {:class    class
          :style (assoc style :padding "0px" :color color)
          :on-click (when-not disabled? #(reset! model date))}
         (merge attr))
     (if (or (not current-month?) current-day?) label [calendar-cell label date])]))

(defn history-datepicker [habit-id model]
  [re-com/datepicker
   :date-cell     #(history-datepicker-cell habit-id model %1)
   :model model
   :on-change #(js/alert "meow")
   :attr (tag :advanced-datepicker-datepicker)])

(defn- format-completion-date [completion]
  (let [[model class]
        (if (:isExactTime completion)
          [(dh/date-time->hours-minutes (:completionDate completion)) ""]
          [0 "timepicker-empty"])]
    [re-com/input-time
     :class class
     :style {:min-height "30px"
             :min-width "45px"}
     :model model
     :on-change #()
     :show-icon? true
     :disabled? true]))

(defn- format-completion-type [completion-type]
  (when completion-type
    [re-com/box
     :class (styles/completion-list-ct-box)
     :style {:border-color (:color completion-type)}
     :child (:name completion-type)]))

(defn completion-history-list-item [habit-id completion-id]
  (let [completion @(<sub [::subs/completion-with-fixed-color completion-id])
        completion-type @(<sub [::subs/completion-type-data completion])
        editing? (r/atom false)]
    [(fn []
       [re-com/h-box
        :class (styles/completion-list-box)
        :style {:border-color (:color completion)}
        :justify :between
        :children
        [[re-com/h-box
          :size "330px"
          :gap "20px"
          :children
          (if (:note completion) ; vertical format, more compact becaues there's note
            [(if completion-type
               [re-com/v-box ; if there's note place time and completion type vertically
                :size "80px"
                :gap "10px"
                :justify :around
                :children
                [[format-completion-date completion]
                 [format-completion-type completion-type]]] ;unless completion has no type
               [format-completion-date completion])
             [re-com/box :child [:p.completion-note (:note completion)]]]
            [[format-completion-date completion] ;horizontal format
             [re-com/box
              :width "240px"
              :child [format-completion-type completion-type]]])]
         [re-com/h-box
          :children
          [[re-com/md-icon-button
            :attr (tag :completion-list-delete)
            :on-click #(>evt [::e/delete-completion habit-id completion-id])
            :md-icon-name "zmdi-delete"]
           [re-com/md-icon-button
            :attr (tag :completion-list-edit)
            :on-click #(reset! editing? true)
            :md-icon-name "zmdi-edit"]]]
         (when @editing?
           [completion-change #(reset! editing? false) completion-id habit-id])]])]))

(defn completion-history-list-panel [habit-id date]
  (let [ids @(<sub [::subs/selected-habit-completion-ids-on-day @date])]
    (when (pos? (count ids))
      [re-com/v-box
       :margin "5px"
       :gap "5px"
       :children
       (map (fn [id] [completion-history-list-item habit-id id]) ids)])))

(defn history-subpanel [habit-id]
  (let [model (r/atom (time/today))]
    [(fn []
       [re-com/h-box
        :children
        [[history-datepicker habit-id model]
         [completion-history-list-panel habit-id model]]])]))

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
                      :completions [history-subpanel id]
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

(defn push-to-the-right [elem]
  [re-com/h-box
   :children
   [[re-com/gap :size "10px"]
    elem]])

(defn radio-group [model title items]
  [re-com/v-box
   :children
   [[re-com/label :label title]
    [re-com/gap :size "10px"]
    [push-to-the-right
     [re-com/v-box
      :children
      (mapv (fn [[id label]]
              [re-com/radio-button
               :model model
               :label label
               :on-change #(reset! model id)
               :value id])
            items)]]]])

(defn ct-delete-popup [close habit-id ct-id]
  (let [note? (r/atom false)
        note (r/atom "")
        color (r/atom :NeverReplace)
        delete (r/atom false)
        tag (add-prefix-tag :delete-popup)
        tr (add-prefix [:ct :delete-popup])]
    [(fn []
       (let [options (cond-> {:delete @delete :color-strategy :NeverReplace}
                       (not @delete) (assoc :color-strategy @color)
                       (and @note? (not @delete)) (assoc :note @note))]
         [re-com/v-box
          :min-width "400px"
          :children
          [[re-com/label :label (tr :title)]
           [re-com/gap :size "20px"]
           [re-com/checkbox
            :attr (tag :delete-checkbox)
            :model delete
            :label (tr :delete)
            :on-change #(swap! delete not)]
           [re-com/gap :size "20px"]
           (when-not @delete
             [:<>
              [re-com/label :label (tr :handle)]
              [push-to-the-right
               [re-com/v-box
                :children
                [[re-com/gap :size "20px"]
                 [radio-group color (tr :color/label)
                  [[:NeverReplace (tr :color/leave)]
                   [:ReplaceOnlyIfNotSet (tr :color/conditional)]
                   [:AlwaysReplace (tr :color/always)]]]
                 [re-com/gap :size "20px"]
                 [re-com/checkbox
                  :attr (tag :note-checkbox)
                  :model note?
                  :label (tr :note)
                  :on-change #(swap! note? not)]
                 [re-com/gap :size "10px"]
                 (when @note? [push-to-the-right
                               [re-com/input-textarea
                                :attr (tag :note)
                                :model note
                                :on-change #(reset! note %)]])]]]])
           [re-com/gap :size "20px"]
           [re-com/h-box
            :justify :between
            :children
            [[re-com/button
              :attr (tag :confirm)
              :class "btn btn-danger"
              :label (tr :confirm)
              :on-click (fn []
                          (>evt [::e/delete-ct habit-id ct-id options])
                          (>evt [::e/locally-remove-habit-completions habit-id])
                          (close))]
             [re-com/button
              :attr (tag :cancel)
              :class "btn btn-secondary"
              :label (tr :cancel)
              :on-click close]]]]]))]))

(defn about-popup [close]
  (let [tr (add-prefix :about)]
    [re-com/v-box
     :width "400px"
     :gap "20px"
     :children [[:h4 (tr :title)]
                [:span (tr :body)]
                [:span (tr :license-pre)
                 [:a {:href "https://www.gnu.org/licenses/agpl-3.0.en.html"} "AGPLv3"]
                 (tr :license-post)]
                [:span (tr :source-at)
                 [:a {:href "https://github.com/am-katzest/simple-habit-tracker-to-show-recruiters-i-can-use-cee-sharp"}
                  (tr :source-at-github-clickable)]]
                [re-com/box
                 :align-self :end
                 :child
                 [re-com/button
                  :class "btn btn-secondary"
                  :attr (tag :about-close)
                  :label (tr :close)
                  :on-click close]]]]))
(def popups
  {:completion-type-delete-options-popup  ct-delete-popup})

(defn popup []
  (when-let [result @(<sub [::subs/popup])]
    (let [[fname args] result]
      (when-let [f (popups fname)]
        (let [close #(>evt [::e/close-popup])]
          [re-com/modal-panel
           :attr (tag :modal-panel)
           :backdrop-on-click close
           :child (into [f close] args)])))))

(defn main-panel []
  (let [current-panel @(<sub [::subs/panel])]
    [:div
     (when-not (= :login current-panel) [navbar current-panel])
     [(or (panels current-panel) error-panel)]
     [popup]
     [alerts]]))
