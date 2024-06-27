(ns frontend.core
  (:require
   [reagent.dom :as rdom]
   [re-frame.core :as re-frame]
   [frontend.events :as events]
   [frontend.views :as views]
   [frontend.config :as config]
   [frontend.persistance :as p]
   [day8.re-frame.http-fx]))

(defn dev-setup []
  (when config/debug?
    (println "dev mode")))

(defn ^:dev/after-load mount-root []
  (re-frame/clear-subscription-cache!)
  (let [root-el (.getElementById js/document "app")]
    (rdom/unmount-component-at-node root-el)
    (rdom/render [views/main-panel] root-el)))

(defn init []
  (if (p/get-token)
    (p/set-panel-if-none-found! :home)
    (p/set-panel! :login))
  (re-frame/dispatch-sync [::events/initialize-db (p/get-panel!)])
  (dev-setup)
  (mount-root))
