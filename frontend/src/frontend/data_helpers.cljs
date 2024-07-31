(ns frontend.data-helpers
  (:require [clojure.string :as str]))

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
