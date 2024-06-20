(ns frontend.events
  (:require
   [re-frame.core :as re-frame]
   [frontend.db :as db]
   [ajax.core :as ajax]))

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
     ;; (js/alert (str [username password]))
     {:http-xhrio (request :post "/users/" [:show] [:show] :params body)})))

(re-frame/reg-event-fx
 :show
 (fn [_ & args]
   (js/alert (str args))))
