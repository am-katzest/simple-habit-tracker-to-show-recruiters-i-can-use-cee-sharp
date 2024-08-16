(ns frontend.data-helpers
  (:require [clojure.string :as str]
            [cljs-time.core :as time]
            [cljs-time.format :as time-format]))

(defn empty-space->nil [s] (when (not= "" s) s))

(defn trim [s]
  (if (some? s) (str/trim s) s))

(defn normalize-habit [h]
  (-> h
      (update :description (comp empty-space->nil trim))
      (update :name trim)))

(defn normalize-ct [ct]
  (-> ct
      (update :description (comp empty-space->nil trim))
      (update :name trim)))

(defn not-empty-string? [s]
  (not (contains? #{"" nil} (trim s))))

(defn nil-or-not-empty-string? [s]
  (or (nil? s) (not (contains? #{""} (trim s)))))

(defn color? [s]
  (re-matches #"^#[a-fA-F0-9]{6}" s))

(defn validate-habit [h]
  (and (not-empty-string? (:name h))
       (nil-or-not-empty-string? (:description h))))

(defn validate-ct [ct]
  (and (not-empty-string? (:name ct))
       (nil-or-not-empty-string? (:description ct))
       (color? (:color ct))))

(defn unparse-date [d]
  (time-format/unparse (time-format/formatters :date-time) (time/to-utc-time-zone d)))

(defn parse-date [d]
  (time/to-default-time-zone (time-format/parse d)))

(defn normalize-completion [c]
  (-> c
      (update :note (comp empty-space->nil trim))))

(defn jsonify-completion [c]
  (update c :completionDate unparse-date))

(defn unjsonify-completion [c]
  (update c :completionDate parse-date))

(defn time-now []
  (time/to-default-time-zone (time/now)))

(defn date-time->hours-minutes [t]
  (let [hour (time/hour t)
        minute (time/minute t)]
    (+ minute (* 100 hour))))

(defn set-hours-minutes [dt hm]
  (doto  (.clone dt)
    (.setSeconds 0)
    (.setMinutes (mod hm 100))
    (.setHours (int (/ hm 100)))))

(defn date->month [date]
  [(time/year date) (time/month date)])

(defn date->month-boundries [date]
  (let [begin (time/first-day-of-the-month date)]
    (map unparse-date [begin (time/plus begin (time/months 1))])))
