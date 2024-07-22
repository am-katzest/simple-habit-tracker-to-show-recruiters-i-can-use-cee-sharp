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

(re-frame/reg-sub
 ::alerts
 (fn [db]
   (:alerts db)))

(re-frame/reg-sub
 ::habits
 (fn [db]
   (:habits-data db)))

(re-frame/reg-sub
 ::habit-names
 :<- [::habits]
 (fn [habits]
   (mapv (fn [[id h]] [id (:name h)]) habits)))

(re-frame/reg-sub
 ::selected-habit
 (fn [db] (-> db :habits :selected-habit)))
