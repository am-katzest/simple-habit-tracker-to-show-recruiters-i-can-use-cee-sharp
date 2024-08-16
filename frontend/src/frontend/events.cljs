(ns frontend.events
  (:require
   [re-frame.core :as re-frame]
   [frontend.db :as db]
   [frontend.localization :refer [tr]]
   [ajax.core :as ajax]
   [frontend.data-helpers :as dh]
   [frontend.alert-formatting :refer [format-alert]]
   [frontend.persistance :as p]))

(defn- wrap-if-kw [thing]
  (if (keyword? thing) [thing] thing))

(defn request [method path on-success on-failure & keywords]
  (apply assoc
         {:method          method
          :uri             (str "/api" path)
          :response-format (ajax/json-response-format {:keywords? true})
          :format (ajax/json-request-format)
          :on-success      (wrap-if-kw on-success)
          :on-failure      (wrap-if-kw on-failure)} keywords))

(defn request-auth [db & other]
  (assoc-in (apply request other)
            [:headers "Authorization"]
            (str "SessionToken " (:token db))))

(defn reg-event-http [keyword f]
  (re-frame/reg-event-fx
   keyword
   (fn [{:keys [db]} [_kw & rest]]
     {:http-xhrio (apply request-auth db (apply f rest))})))

(re-frame/reg-event-db
 ::initialize-db
 (fn [_ [_ token panel]]
   (assoc db/default-db :panel panel :token token)))

(re-frame/reg-event-fx
 ::create-new-account
 (fn [_ [_ username password]]
   (let [body {:login username
               :password password}]
     {:http-xhrio (request :post "/users/" [::ask-for-token username password] [::http-error {:data body}] :params body)})))

(re-frame/reg-event-fx
 ::ask-for-token
 (fn [_ [_ username password]]
   (let [body {:login username
               :password password}]
     {:http-xhrio (request :post "/users/createtoken" [::receive-token] [::http-error {:data body}] :params body)})))

(re-frame/reg-event-fx
 ::ask-for-token-login
 (fn [_ [_ username password]]
   (let [body {:login username
               :password password}]
     {:http-xhrio (request :post "/users/createtoken" [::receive-token] [::http-error {:data body}] :params body)})))

(re-frame/reg-event-fx
 ::receive-token
 (fn [{:keys [db]} [_ token]]
   (let [panel (if (= :login (:panel db))
                 :habits
                 (:panel db))]
     {:dispatch [::reset-panel]
      :db (assoc db :token token :panel panel)
      :store-token token
      :set-panel panel})))

(re-frame/reg-event-fx
 ::reset-panel
 (fn [{:keys [db]}]
   (let [panel (:panel db)
         panel-setter ({:habits [::habits-panel]
                        :account [::account-panel]} panel)]
     (if panel-setter {:dispatch panel-setter} {}))))

(reg-event-http
 ::download-user
 (fn [] [:get "/users/me" [::receive-user] [::http-error]]))

(re-frame/reg-event-db
 ::receive-user
 (fn [db [_ user]]
   (assoc db :user user)))

(re-frame/reg-event-fx
 ::logout
 (fn [_ [_]]
   {:db (assoc db/default-db :panel :login)
    :store-token nil
    :set-panel :login}))

(re-frame/reg-event-fx
 ::http-error
 (fn [_ [_ arg1 arg2]]
   ;;  last argument is response
   ;;  second argument has context for formatting, it is optional
   (let [[response {:keys [handler data]}] (if arg2
                                             [arg2 arg1]
                                             [arg1 {}])]
     {:fx [(when handler [:dispatch handler])
           [:dispatch [::add-alert (format-alert (:response response) data)]]]})))

(re-frame/reg-event-fx
 ::account-panel
 (fn [{:keys [db]} & _]
   {:db (assoc db :panel :account)
    :set-panel :account
    :dispatch [::download-user]}))

(re-frame/reg-event-fx
 ::habits-panel
 (fn [{:keys [db]} & _]
   {:db (assoc db :panel :habits)
    :dispatch [::download-habits]
    :set-panel :habits}))

(re-frame/reg-event-db
 ::close-alert
 (fn [db [_ id]]
   (update db :alerts (fn [a] (remove #(= id (:id %)) a)))))

(re-frame/reg-event-db
 ::add-alert
 (fn [db [_ alert]]
   (let [id (->> db :alerts (map :id) (reduce max 0) inc)]
     (update db :alerts conj (assoc alert :id id)))))

(re-frame/reg-event-fx
 ::select-habit
 (fn [{:keys [db]} [_ id]]
   (cond-> {:db (assoc-in db [:habits :selected-habit] id)}
     (some? id) (assoc :fx [[:dispatch [::download-habit-details id]]
                            [:dispatch [::download-habit-cts id]]]))))

(reg-event-http
 ::download-habits
 (fn [] [:get "/habits/" ::receive-habits [::http-error]]))

(re-frame/reg-event-fx
 ::receive-habits
 (fn [{:keys [db]} [_ habits]]
   (let [formatted (->> habits (map (fn [{:keys [id] :as h}] [id h])) (into {}))]
     {:db (assoc db :habits-data formatted)
      :dispatch [::fixup-habit-selection]})))

(reg-event-http
 ::new-empty-habit
 (fn []
   (let [data (dh/normalize-habit {:name (tr :habit/new-habit)})]
     [:post "/habits/" [::empty-habit-created data] ::http-error :params data])))

(re-frame/reg-event-db
 ::empty-habit-created
 (fn [db [_ habit-no-id id]]
   (let [habit (assoc habit-no-id :id id)]
     (-> db
         (update :habits-data assoc id habit)
         (assoc-in [:habits :selected-habit] id)))))

(reg-event-http
 ::download-habit-details
 (fn [id]
   [:get (str "/habits/" id) [::receive-habit-details id] [::http-error {:handler [::download-habits]}]]))

(re-frame/reg-event-db
 ::receive-habit-details
 (fn [db [_ id habit-details]]
   (update db :habits-data assoc id habit-details)))

(reg-event-http
 ::update-habit
 (fn [habit]
   [:put (str "/habits/" (:id habit)) [::receive-habit-details (:id habit) habit] [::http-error {:handler [::download-habits]}]
    :params (dissoc habit :id)]))

(reg-event-http
 ::delete-habit
 (fn [id]
   [:delete (str "/habits/" id) [::delete-habit-receive id] [::http-error {:handler [::download-habits]}]]))

(re-frame/reg-event-fx
 ::delete-habit-receive
 (fn [{:keys [db]} [_ id]]
   {:db (update db :habits-data dissoc id)
    :dispatch [::fixup-habit-selection]}))

(re-frame/reg-event-fx
 ::fixup-habit-selection
 (fn [{:keys [db]} _]
   (let [old-id (get-in db [:habits :selected-habit])
         existing-data (get-in db [:habits-data old-id])
         other-id (-> db :habits-data ffirst) ; will be null if no habits remain
         new-id (when (some? other-id)
                  (if (some? existing-data) old-id other-id))]
     (when (not= new-id old-id)
       {:dispatch [::select-habit new-id]}))))

(reg-event-http
 ::download-habit-cts
 (fn [id]
   [:get (str "/habits/" id "/completionTypes/") [::receive-habit-cts id] [::http-error {:handler [::download-habits]}]]))

(re-frame/reg-event-db
 ::receive-habit-cts
 (fn [db [_ habit-id cts]]
   (-> db
       (assoc-in [:cts habit-id]
                 (into {} (map (juxt :id identity) cts)))
       (assoc-in [:habits habit-id :selected-ct] (:id (first cts))))))

(reg-event-http
 ::new-empty-ct
 (fn [id]
   (let [color "#555555"
         name (tr :ct/new-ct)
         new-ct (dh/normalize-ct {:name name :color color})]
     [:post (str "/habits/" id "/completionTypes/") [::receive-newly-created-ct id new-ct] [::http-error {:handler [::download-habits]}] :params new-ct])))

(re-frame/reg-event-db
 ::receive-newly-created-ct
 (fn [db [_ hid new-ct ctid]]
   (-> db
       (assoc-in [:cts hid ctid] (assoc new-ct :id ctid))
       (assoc-in [:habits hid :selected-ct] ctid))))

(re-frame/reg-event-db
 ::select-ct
 (fn [db [_ hid ctid]]
   (assoc-in db [:habits hid :selected-ct] ctid)))

(reg-event-http
 ::delete-ct
 (fn [hid ctid]
   [:delete (str "/habits/" hid "/completionTypes/" ctid) [::delete-ct-receive hid ctid] [::http-error {:handler [::download-habit-cts hid]}]]))

(re-frame/reg-event-db
 ::delete-ct-receive
 (fn [db [_ hid ctid]]
   (let [cts (dissoc (get-in db [:cts hid]) ctid)]
     (-> db
         (assoc-in [:cts hid] cts)
         (assoc-in [:habits hid :selected-ct] (ffirst cts))))))

(reg-event-http
 ::update-ct
 (fn [hid ctid ct]
   [:put (str "/habits/" hid "/CompletionTypes/" ctid) [::confirm-ct-update hid ctid ct] [::http-error {:handler [::download-habit-cts hid]}]
    :params (dissoc ct :id)]))

(re-frame/reg-event-db
 ::confirm-ct-update
 (fn [db [_ hid ctid ct]]
   (assoc-in db [:cts hid ctid] ct)))

(reg-event-http
 ::add-completion
 (fn [id body]
   [:post (str "/habits/" id "/Completions/") [::confirm-completion-add id body] [::http-error] :params (dh/jsonify-completion body)]))

(re-frame/reg-event-db
 ::confirm-completion-add
 (fn [db [_ habit-id body completion-id]]
   (assoc-in db [:completions habit-id completion-id] (assoc body :id completion-id))))

(reg-event-http
 ::edit-completion
 (fn [habit-id completion-id body]
   [:put (str "/habits/" habit-id "/Completions/" completion-id) [::confirm-completion-add habit-id body completion-id] [::http-error] :params (dh/jsonify-completion body)]))
(re-frame/reg-event-fx
 ::ensure-completion-history-month-is-downloaded
 (fn [{:keys [db]} [_ habit-id date]]
   (let [path [:completion-download-statuses habit-id (dh/date->month date)]
         existing (get-in db path)]
     (when-not existing
       {:db (assoc-in db path :pending)
        :dispatch [::download-month-of-completion-history habit-id date]}))))

(reg-event-http
 ::download-month-of-completion-history
 (fn [habit-id date]
   (println date)
   (let [[after before] (dh/date->month-boundries date)]
     [:get (str "/habits/" habit-id "/completions/?before=" before "&after=" after)
      [::download-month-of-completion-history-success habit-id date]
      [::download-month-of-completion-history-failure habit-id date]])))

(re-frame/reg-event-db
 ::download-month-of-completion-history-success
 (fn [db [_ habit-id date completions]]
   (let [fixed-completions
         (->> completions
              (map dh/unjsonify-completion)
              (map (juxt :id identity))
              (into {}))]
     (-> db
         (assoc-in [:completion-download-statuses habit-id (dh/date->month date)] :downloaded)
         (update-in [:completions habit-id] merge fixed-completions)))))

(re-frame/reg-event-db
 ::download-month-of-completion-history-failure
 (fn [db [_ habit-id date]]
   (assoc-in db [:completion-download-statuses habit-id (dh/date->month date)] nil)))
