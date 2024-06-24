(ns frontend.subs
  (:require
   [re-frame.core :as re-frame]))

(re-frame/reg-sub
 ::name
 (fn [db]
   (:name db)))

(re-frame/reg-sub
 ::panel
 (fn [db]
   (:panel db)))

(re-frame/reg-sub
 :locale/map
 (fn [{:keys [locale locales]}]
   (locale locales)))

(re-frame/reg-sub
 ::user
 (fn [db]
   (:user db)))
