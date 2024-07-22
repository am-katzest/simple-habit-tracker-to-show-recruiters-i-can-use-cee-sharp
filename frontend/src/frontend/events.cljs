(ns frontend.events
  (:require
   [re-frame.core :as re-frame]
   [frontend.db :as db]
   [frontend.localization :refer [tr]]
   [ajax.core :as ajax]
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
     {:http-xhrio (request :post "/users/" [::ask-for-token username password] [:show] :params body)})))

(re-frame/reg-event-fx
 ::ask-for-token
 (fn [_ [_ username password]]
   (let [body {:login username
               :password password}]
     {:http-xhrio (request :post "/users/createtoken" [::receive-token] [:show] :params body)})))

(re-frame/reg-event-fx
 ::ask-for-token-login
 (fn [_ [_ username password]]
   (let [body {:login username
               :password password}]
     {:http-xhrio (request :post "/users/createtoken" [::receive-token] [::add-alert [:danger :body (tr :error/wrong-cred)]] :params body)})))

(re-frame/reg-event-fx
 ::receive-token
 (fn [{:keys [db]} [_ token]]
   (let [panel (if (= :login (:panel db))
                 :habits
                 (:panel db))
         db (assoc db :token token)]
     {:dispatch [::reset-panel]
      :db db
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
 (fn [] [:get "/users/me" [::receive-user] [:show]]))

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
 :show
 (fn [_ & args]
   (js/alert (str args))))

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
 (fn [db [_ [type & keys] & _]]
   (let [id (->> db :alerts (map :id) (reduce max 0) inc)
         new-alert (apply assoc {:alert-type type :id id} keys)]
     (update db :alerts conj new-alert))))

(re-frame/reg-event-fx
 ::select-habit
 (fn [{:keys [db]} [_ id]]
   {:db (assoc-in db [:habits :selected-habit] id)}))

(reg-event-http
 ::download-habits
 (fn [] [:get "/habits/" ::receive-habits :show]))

(re-frame/reg-event-fx
 ::receive-habits
 (fn [{:keys [db]} [_ habits]]
   (let [formatted (->> habits (map (fn [{:keys [id] :as h}] [id h])) (into {}))]
     {:db (assoc db :habits-data formatted)})))

(reg-event-http
 ::new-empty-habit
 (fn []
   (let [data {:name "new habit"}]
     [:post "/habits/" [::empty-habit-created data] :show :params data])))

(re-frame/reg-event-db
 ::empty-habit-created
 (fn [db [_ habit-no-id id]]
   (let [habit (assoc habit-no-id :id id)]
     (-> db
         (update :habits-data assoc id habit)
         (assoc-in [:habits :selected-habit] id)))))
