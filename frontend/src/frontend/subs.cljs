(ns frontend.subs
  (:require
   [cljs-time.core :as time]
   [frontend.data-helpers :as dh]
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
 ::popup
 (fn [db]
   (:popup db)))

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
 ::habits-state
 (fn [db]
   (:habits db)))

(re-frame/reg-sub
 ::habit-names
 :<- [::habits]
 (fn [habits]
   (mapv (fn [[id h]] [id (:name h)]) habits)))

(re-frame/reg-sub
 ::selected-habit
 (fn [db] (-> db :habits :selected-habit)))

(re-frame/reg-sub
 ::habit
 :<- [::habits]
 (fn [habits [_ id]]
   (when (and habits id) (habits id))))

(re-frame/reg-sub
 ::all-cts
 (fn [db]
   (:cts db)))

(re-frame/reg-sub
 ::selected-habit-cts
 :< [::all-cts]
 :< [::selected-habit]
 (fn [[all selected]]
   (when (and all selected)
     (all selected))))

(re-frame/reg-sub
 ::selected-ct
 :< [::habits-state]
 :< [::selected-habit]
 (fn [[state id]] (get-in state [id :selected-ct])))

(re-frame/reg-sub
 ::selected-ct-data
 :<- [::selected-habit-cts]
 :<- [::selected-ct]
 (fn [[all id]]
   (when (and all id) (all id))))

(re-frame/reg-sub
 ::all-completions
 (fn [db]
   (:completions db)))

(re-frame/reg-sub
 ::selected-habit-completions
 :<- [::all-completions]
 :<- [::selected-habit]
 (fn [[all id]]
   (when (and all id) (all id))))

(re-frame/reg-sub
 ::selected-habit-completion-ids-on-day
 :<- [::selected-habit-completions]
 (fn [completions [_ day]]
   (keep (fn [[id {:keys [completionDate]}]]
           (when (and  (= (time/day completionDate) (time/day day))
                       (= (time/month completionDate) (time/month day))
                       (= (time/year completionDate) (time/year day))) id))
         completions)))

(re-frame/reg-sub
 ::completion-with-fixed-color
 :<- [::selected-habit-cts]
 :<- [::selected-habit-completions]
 (fn [[completion-types completions] [_ id]]
   ;; try completion own color, completion type color and then black
   (let [comp (completions id)]
     (assoc comp :color
            (or (:color comp)
                (-> comp :completionTypeId completion-types :color)
                :black)))))

(re-frame/reg-sub
 ::completion
 :<- [::selected-habit-completions]
 (fn [completions [_ id]]
   (completions id)))

(re-frame/reg-sub
 ::completion-type-data
 :<- [::selected-habit-cts]
 (fn [all [_ comp]]
   (let [id (:completionTypeId comp)]
     (when (and all id) (all id)))))
