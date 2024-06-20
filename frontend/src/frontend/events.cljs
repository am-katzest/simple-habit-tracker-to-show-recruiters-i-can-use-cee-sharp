(ns frontend.events
  (:require
   [re-frame.core :as re-frame]
   [frontend.db :as db]
   [ajax.core :as ajax]
   [frontend.persistance :as p]))

(defn request [method path on-success on-failure & keywords]
  (apply assoc
         {:method          method
          :uri             (str "/api" path)
          :response-format (ajax/json-response-format {:keywords? true})
          :format (ajax/json-request-format)
          :on-success      on-success
          :on-failure      on-failure} keywords))

(defn request-auth [db & other]
  (assoc-in (apply request other)
            [:headers "Authorization"]
            (str "SessionToken " (:token db))))

(re-frame/reg-event-db
 ::initialize-db
 (fn [_ [_ panel]]
   (assoc db/default-db :panel panel)))

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
 ::receive-token
 (fn [{:keys [db]} [_ token]]
   (let [panel (if (= :login (:panel db))
                 :home
                 (:panel db))
         db (assoc db :token token :panel panel)]
     (cond->
      {:db db
       :store-token token
       :set-panel panel}
       (nil? (:user db))
       (assoc :http-xhrio (request-auth db :get "/users/me" [::receive-user] [:show]))))))

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
