(ns frontend-test.helpers
  (:require [frontend-test.setup :as s :refer-macros true]
            [etaoin.api :as e]
            [clojure.test :refer [is]]))

(defn tag-selector-factory-factory [x]
  (fn self
    ([] x)
    ([id] (assoc (self) :data-testid id))
    ([id enabled] (assoc (self id) :fn/enabled enabled))))

(def any (tag-selector-factory-factory {}))

(def btn (tag-selector-factory-factory {:tag :button}))

(def input (tag-selector-factory-factory {:tag :input}))

(def textarea (tag-selector-factory-factory {:tag :textarea}))

(defn list-item [text]
  (format "//div[contains(@class, 'list-group')]//descendant::*[text()='%s']" text))

(defn completion-list-item [text]
  (format "//p[contains(text(),'%s')]/../../.." text))
(defn descendant-with-class [c] (format "/descendant::*[contains(@class,'%s')]" (name c)))
(defn descendant-with-tag [t] (format "/descendant::*[(@data-testid='%s')]" (name t)))

(defn wrap-waiter-into-test [f]
  (fn [& args]
    (is (any? (apply f s/*driver* args)) args)))

(def wait-enabled (wrap-waiter-into-test e/wait-enabled))
(def wait-exists (wrap-waiter-into-test e/wait-exists))
(def wait-disabled (wrap-waiter-into-test e/wait-disabled))
(def wait-predicate e/wait-predicate)   ; one of the functions ignoring convention

(defn lazy-is' "tries predicate until it succeds, after timeout throws last exception" [pred]
  (let [captured-ex (atom nil)]
    (try (wait-predicate
          (fn []
            (try (pred)
                 (catch Throwable inner
                   (reset! captured-ex inner)
                   nil))))
         (catch clojure.lang.ExceptionInfo e
           (if (and (= :etaoin/timeout (:type (ex-data e)))
                    (some? @captured-ex))
             (throw @captured-ex) :timeout)))))

(defmacro lazy-is
  "version of is repeatedly trying predicate until it succeeds"
  [form & rest]
  `(is (= nil (lazy-is' (fn [] ~form))) ~@rest))

(defn fix [x]
  (if (keyword? x) (any x) x))

(defmacro exists? [target & rest]
  `(lazy-is (e/exists? s/*driver* (fix ~target)) ~@rest))

(defmacro absent? [target & rest]
  `(lazy-is (e/absent? s/*driver* (fix ~target)) ~@rest))

(defn query [s]
  (reduce str (drop-while (complement #{\?}) s)))

(def short-wait
  {:timeout 0.5
   :interval 0.01})
(defmacro with-wait [params & exprs]
  `(binding [e/*wait-timeout* (:timeout ~params)
             e/*wait-interval* (:interval ~params)]
     ~@exprs))

(defn random-str []
  (reduce str (take 15 (str (random-uuid)))))

(defn join [a b]
  (keyword (str (name a) "-" (name b))))
